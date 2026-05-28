using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.Services;
using System.Security.Claims;

namespace PayrollAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayslipController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPayslipPdfService _pdfService;

        public PayslipController(
            AppDbContext context,
            IPayslipPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // GET /api/payslip/employee/1
        // Employee sees only their own, HR sees all
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRoles = User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value).ToList();

            var isAdminRole = userRoles.Any(r =>
                r == "SuperAdmin" || r == "HRAdmin" || r == "FinanceOfficer");

            // If employee role, verify they are viewing their own payslips
            if (!isAdminRole)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == currentUserEmail);

                var appUser = user as AppUser;
                if (appUser?.EmployeeId != employeeId)
                    return Forbid();
            }

            var payslips = await _context.Payslips
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.PayPeriod)
                .Select(p => new
                {
                    p.Id,
                    p.PayPeriod,
                    p.GrossPay,
                    p.TotalDeductions,
                    p.NetPay,
                    p.GeneratedAt
                })
                .ToListAsync();

            return Ok(payslips);
        }

        // GET /api/payslip/1/download — download PDF
        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(int id)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRoles = User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value).ToList();

            var isAdminRole = userRoles.Any(r =>
                r == "SuperAdmin" || r == "HRAdmin" || r == "FinanceOfficer");

            // Get the payslip with employee data
            var payslip = await _context.Payslips
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payslip == null)
                return NotFound(new { message = "Payslip not found" });

            // Security check — employees can only download their own
            if (!isAdminRole)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == currentUserEmail)
                    as AppUser;

                if (user?.EmployeeId != payslip.EmployeeId)
                    return Forbid();
            }

            // Generate the PDF
            var pdfBytes = await _pdfService
                .GeneratePayslipPdfAsync(payslip, payslip.Employee!);

            // Return as downloadable file
            return File(
                pdfBytes,
                "application/pdf",
                $"Payslip_{payslip.Employee!.LastName}" +
                $"_{payslip.PayPeriod}.pdf");
        }

        // GET /api/payslip/run/1 — get all payslips for a payroll run
        [HttpGet("run/{payrollRunId}")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,FinanceOfficer")]
        public async Task<IActionResult> GetByPayrollRun(int payrollRunId)
        {
            var payslips = await _context.Payslips
                .Include(p => p.Employee)
                .Where(p => p.PayrollRunId == payrollRunId)
                .OrderBy(p => p.Employee!.LastName)
                .Select(p => new
                {
                    p.Id,
                    p.EmployeeId,
                    EmployeeName = $"{p.Employee!.FirstName} {p.Employee.LastName}",
                    p.Employee.Department,
                    p.BasicSalary,
                    p.GrossPay,
                    p.TotalDeductions,
                    p.NetPay,
                    p.PayPeriod
                })
                .ToListAsync();

            return Ok(payslips);
        }
    }
}