using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs;
using PayrollAPI.Helpers;
using PayrollAPI.Models;

namespace PayrollAPI.Services
{
    public interface ISalaryService
    {
        Task<SalaryResponseDto?> GetSalaryByEmployeeIdAsync(int employeeId);
        Task<SalaryResponseDto> CreateOrUpdateSalaryAsync(
            CreateSalaryDto dto, string updatedByEmail);
        Task<TaxResult> PreviewTaxCalculationAsync(int employeeId);
    }

    public class SalaryService : ISalaryService
    {
        private readonly AppDbContext _context;

        public SalaryService(AppDbContext context)
        {
            _context = context;
        }

        // GET SALARY FOR ONE EMPLOYEE
        public async Task<SalaryResponseDto?> GetSalaryByEmployeeIdAsync(
            int employeeId)
        {
            var salary = await _context.Salaries
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId
                                       && s.IsActive);

            if (salary == null) return null;

            return MapToDto(salary);
        }

        // CREATE OR UPDATE SALARY
        public async Task<SalaryResponseDto> CreateOrUpdateSalaryAsync(
            CreateSalaryDto dto, string updatedByEmail)
        {
            // Check employee exists
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                throw new Exception($"Employee with ID {dto.EmployeeId} not found");

            // Deactivate any existing salary
            var existingSalary = await _context.Salaries
                .FirstOrDefaultAsync(s => s.EmployeeId == dto.EmployeeId
                                       && s.IsActive);

            if (existingSalary != null)
                existingSalary.IsActive = false;

            // Create new salary record
            var salary = new Salary
            {
                EmployeeId = dto.EmployeeId,
                BasicSalary = dto.BasicSalary,
                HousingAllowance = dto.HousingAllowance,
                TransportAllowance = dto.TransportAllowance,
                MealAllowance = dto.MealAllowance,
                OtherAllowances = dto.OtherAllowances,
                EffectiveDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Salaries.Add(salary);

            // Log the action
            _context.AuditLogs.Add(new AuditLog
            {
                UserEmail = updatedByEmail,
                UserId = updatedByEmail,
                Action = "SET_SALARY",
                Entity = "Salary",
                Details = $"Set salary for Employee ID {dto.EmployeeId}. " +
                          $"Basic: {dto.BasicSalary:N2}",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            salary.Employee = employee;
            return MapToDto(salary);
        }

        // PREVIEW TAX CALCULATION BEFORE RUNNING PAYROLL
        public async Task<TaxResult> PreviewTaxCalculationAsync(int employeeId)
        {
            var salary = await _context.Salaries
                .FirstOrDefaultAsync(s => s.EmployeeId == employeeId
                                       && s.IsActive);

            if (salary == null)
                throw new Exception(
                    $"No active salary found for Employee ID {employeeId}");

            return TaxCalculator.Calculate(
                salary.BasicSalary,
                salary.HousingAllowance,
                salary.TransportAllowance,
                salary.MealAllowance,
                salary.OtherAllowances
            );
        }

        private static SalaryResponseDto MapToDto(Salary s)
        {
            return new SalaryResponseDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                EmployeeName = s.Employee != null
                    ? $"{s.Employee.FirstName} {s.Employee.LastName}"
                    : string.Empty,
                BasicSalary = s.BasicSalary,
                HousingAllowance = s.HousingAllowance,
                TransportAllowance = s.TransportAllowance,
                MealAllowance = s.MealAllowance,
                OtherAllowances = s.OtherAllowances,
                GrossSalary = s.GrossSalary,
                EffectiveDate = s.EffectiveDate
            };
        }
    }
}