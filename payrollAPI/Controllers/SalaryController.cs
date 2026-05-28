using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs;
using PayrollAPI.Services;
using System.Security.Claims;

namespace PayrollAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalaryController : ControllerBase
    {
        private readonly ISalaryService _salaryService;

        public SalaryController(ISalaryService salaryService)
        {
            _salaryService = salaryService;
        }

        // GET /api/salary/employee/5
        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,FinanceOfficer")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var salary = await _salaryService
                .GetSalaryByEmployeeIdAsync(employeeId);

            if (salary == null)
                return NotFound(new {
                    message = $"No salary found for employee ID {employeeId}"
                });

            return Ok(salary);
        }

        // POST /api/salary — create or update salary
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,HRAdmin")]
        public async Task<IActionResult> CreateOrUpdate(
            [FromBody] CreateSalaryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email)
                ?? "system";

            try
            {
                var salary = await _salaryService
                    .CreateOrUpdateSalaryAsync(dto, currentUserEmail);
                return Ok(salary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/salary/preview/5 — preview tax before running payroll
        [HttpGet("preview/{employeeId}")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,FinanceOfficer")]
        public async Task<IActionResult> PreviewTax(int employeeId)
        {
            try
            {
                var result = await _salaryService
                    .PreviewTaxCalculationAsync(employeeId);

                return Ok(new
                {
                    grossSalary = result.GrossSalary,
                    basicSalary = result.BasicSalary,
                    totalAllowances = result.TotalAllowances,
                    deductions = new
                    {
                        employeePension = result.EmployeePension,
                        employerPension = result.EmployerPension,
                        nhf = result.NHF,
                        paye = result.PAYE,
                        total = result.TotalDeductions
                    },
                    netPay = result.NetPay
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}