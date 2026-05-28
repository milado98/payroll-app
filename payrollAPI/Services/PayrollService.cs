using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs;
using PayrollAPI.Helpers;
using PayrollAPI.Models;

namespace PayrollAPI.Services
{
    public interface IPayrollService
    {
        Task<PayrollRunResponseDto> CreatePayrollRunAsync(
            string payPeriod, string createdByEmail);
        Task<PayrollRunResponseDto?> GetPayrollRunAsync(int id);
        Task<List<PayrollRunResponseDto>> GetAllPayrollRunsAsync();
        Task<PayrollRunResponseDto?> ApprovePayrollRunAsync(
            int id, string approvedByEmail);
        Task<bool> DeleteDraftPayrollRunAsync(
            int id, string deletedByEmail);
    }

    public class PayrollService : IPayrollService
    {
        private readonly AppDbContext _context;

        public PayrollService(AppDbContext context)
        {
            _context = context;
        }

        // ── CREATE PAYROLL RUN ──
        public async Task<PayrollRunResponseDto> CreatePayrollRunAsync(
            string payPeriod, string createdByEmail)
        {
            // SAFETY CHECK 1: Cannot run payroll twice for same period
            var exists = await _context.PayrollRuns
                .AnyAsync(p => p.PayPeriod == payPeriod);

            if (exists)
                throw new Exception(
                    $"Payroll for {payPeriod} has already been run. " +
                    $"Cannot create duplicate.");

            // SAFETY CHECK 2: Must have active employees with salaries
            var employees = await _context.Employees
                .Include(e => e.Salary)
                .Where(e => e.IsActive && e.Salary != null
                         && e.Salary.IsActive)
                .ToListAsync();

            if (!employees.Any())
                throw new Exception(
                    "No active employees with salaries found. " +
                    "Please assign salaries before running payroll.");

            // CREATE THE PAYROLL RUN RECORD
            var payrollRun = new PayrollRun
            {
                PayPeriod = payPeriod,
                Status = "DRAFT",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByEmail
            };

            _context.PayrollRuns.Add(payrollRun);
            await _context.SaveChangesAsync();

            // GENERATE A PAYSLIP FOR EACH EMPLOYEE
            var payslips = new List<Payslip>();
            decimal totalGross = 0;
            decimal totalDeductions = 0;
            decimal totalNet = 0;

            foreach (var employee in employees)
            {
                var salary = employee.Salary!;

                // Run the tax calculator for this employee
                var tax = TaxCalculator.Calculate(
                    salary.BasicSalary,
                    salary.HousingAllowance,
                    salary.TransportAllowance,
                    salary.MealAllowance,
                    salary.OtherAllowances
                );

                var payslip = new Payslip
                {
                    EmployeeId = employee.Id,
                    PayrollRunId = payrollRun.Id,
                    PayPeriod = payPeriod,
                    BasicSalary = salary.BasicSalary,
                    TotalAllowances = tax.TotalAllowances,
                    GrossPay = tax.GrossSalary,
                    PAYE = tax.PAYE,
                    Pension = tax.EmployeePension,
                    EmployerPension = tax.EmployerPension,
                    NHF = tax.NHF,
                    OtherDeductions = 0,
                    TotalDeductions = tax.TotalDeductions,
                    NetPay = tax.NetPay,
                    GeneratedAt = DateTime.UtcNow
                };

                payslips.Add(payslip);
                totalGross += tax.GrossSalary;
                totalDeductions += tax.TotalDeductions;
                totalNet += tax.NetPay;
            }

            // SAVE ALL PAYSLIPS
            _context.Payslips.AddRange(payslips);

            // UPDATE PAYROLL RUN TOTALS
            payrollRun.TotalGrossPay = totalGross;
            payrollRun.TotalDeductions = totalDeductions;
            payrollRun.TotalNetPay = totalNet;

            await _context.SaveChangesAsync();

            // LOG THE ACTION
            _context.AuditLogs.Add(new AuditLog
            {
                UserEmail = createdByEmail,
                UserId = createdByEmail,
                Action = "CREATED_PAYROLL_RUN",
                Entity = "PayrollRun",
                Details = $"Created payroll run for {payPeriod}. " +
                          $"Employees: {employees.Count}, " +
                          $"Total Net: {totalNet:N2}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return await BuildResponseDto(payrollRun.Id);
        }

        // ── GET ONE PAYROLL RUN ──
        public async Task<PayrollRunResponseDto?> GetPayrollRunAsync(int id)
        {
            var exists = await _context.PayrollRuns.AnyAsync(p => p.Id == id);
            if (!exists) return null;
            return await BuildResponseDto(id);
        }

        // ── GET ALL PAYROLL RUNS ──
        public async Task<List<PayrollRunResponseDto>> GetAllPayrollRunsAsync()
        {
            var runs = await _context.PayrollRuns
                .OrderByDescending(p => p.PayPeriod)
                .ToListAsync();

            var result = new List<PayrollRunResponseDto>();
            foreach (var run in runs)
                result.Add(await BuildResponseDto(run.Id));

            return result;
        }

        // ── APPROVE PAYROLL RUN ──
        public async Task<PayrollRunResponseDto?> ApprovePayrollRunAsync(
            int id, string approvedByEmail)
        {
            var payrollRun = await _context.PayrollRuns.FindAsync(id);
            if (payrollRun == null) return null;

            // Can only approve a DRAFT
            if (payrollRun.Status != "DRAFT")
                throw new Exception(
                    $"Cannot approve payroll with status '{payrollRun.Status}'. " +
                    $"Only DRAFT payrolls can be approved.");

            // Cannot approve your own payroll run (4-eyes principle)
            if (payrollRun.CreatedBy == approvedByEmail)
                throw new Exception(
                    "You cannot approve a payroll run that you created. " +
                    "A different user must approve it.");

            payrollRun.Status = "APPROVED";
            payrollRun.ApprovedAt = DateTime.UtcNow;
            payrollRun.ApprovedBy = approvedByEmail;

            _context.AuditLogs.Add(new AuditLog
            {
                UserEmail = approvedByEmail,
                UserId = approvedByEmail,
                Action = "APPROVED_PAYROLL_RUN",
                Entity = "PayrollRun",
                Details = $"Approved payroll run for {payrollRun.PayPeriod}. " +
                          $"Total Net Pay: {payrollRun.TotalNetPay:N2}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return await BuildResponseDto(id);
        }

        // ── DELETE DRAFT (only drafts can be deleted) ──
        public async Task<bool> DeleteDraftPayrollRunAsync(
            int id, string deletedByEmail)
        {
            var payrollRun = await _context.PayrollRuns
                .Include(p => p.Payslips)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payrollRun == null) return false;

            if (payrollRun.Status != "DRAFT")
                throw new Exception(
                    "Only DRAFT payroll runs can be deleted. " +
                    "Approved payrolls are permanent.");

            // Remove payslips first then the run
            _context.Payslips.RemoveRange(payrollRun.Payslips);
            _context.PayrollRuns.Remove(payrollRun);

            _context.AuditLogs.Add(new AuditLog
            {
                UserEmail = deletedByEmail,
                UserId = deletedByEmail,
                Action = "DELETED_PAYROLL_RUN",
                Entity = "PayrollRun",
                Details = $"Deleted DRAFT payroll run for " +
                          $"{payrollRun.PayPeriod}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        // ── HELPER: Build the full response DTO ──
        private async Task<PayrollRunResponseDto> BuildResponseDto(int id)
        {
            var run = await _context.PayrollRuns
                .Include(p => p.Payslips)
                    .ThenInclude(ps => ps.Employee)
                .FirstAsync(p => p.Id == id);

            return new PayrollRunResponseDto
            {
                Id = run.Id,
                PayPeriod = run.PayPeriod,
                Status = run.Status,
                CreatedAt = run.CreatedAt,
                ApprovedAt = run.ApprovedAt,
                CreatedBy = run.CreatedBy,
                ApprovedBy = run.ApprovedBy,
                TotalGrossPay = run.TotalGrossPay,
                TotalDeductions = run.TotalDeductions,
                TotalNetPay = run.TotalNetPay,
                TotalEmployees = run.Payslips.Count,
                Payslips = run.Payslips.Select(ps => new PayslipSummaryDto
                {
                    Id = ps.Id,
                    EmployeeId = ps.EmployeeId,
                    EmployeeName = ps.Employee != null
                        ? $"{ps.Employee.FirstName} {ps.Employee.LastName}"
                        : string.Empty,
                    Department = ps.Employee?.Department ?? string.Empty,
                    BasicSalary = ps.BasicSalary,
                    TotalAllowances = ps.TotalAllowances,
                    GrossPay = ps.GrossPay,
                    EmployeePension = ps.Pension,
                    EmployerPension = ps.EmployerPension,
                    NHF = ps.NHF,
                    PAYE = ps.PAYE,
                    TotalDeductions = ps.TotalDeductions,
                    NetPay = ps.NetPay
                }).ToList()
            };
        }
    }
}