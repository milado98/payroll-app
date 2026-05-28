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
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        // GET /api/payroll — get all payroll runs
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,HRAdmin,FinanceOfficer")]
        public async Task<IActionResult> GetAll()
        {
            var runs = await _payrollService.GetAllPayrollRunsAsync();
            return Ok(runs);
        }

        // GET /api/payroll/5 — get one payroll run
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,FinanceOfficer")]
        public async Task<IActionResult> GetById(int id)
        {
            var run = await _payrollService.GetPayrollRunAsync(id);
            if (run == null)
                return NotFound(new {
                    message = $"Payroll run with ID {id} not found"
                });

            return Ok(run);
        }

        // POST /api/payroll/run — create a new payroll run
        [HttpPost("run")]
        [Authorize(Roles = "SuperAdmin,HRAdmin")]
        public async Task<IActionResult> RunPayroll(
            [FromBody] CreatePayrollRunDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email)
                ?? "system";

            try
            {
                var run = await _payrollService
                    .CreatePayrollRunAsync(dto.PayPeriod, currentUserEmail);

                return CreatedAtAction(nameof(GetById),
                    new { id = run.Id }, run);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH /api/payroll/5/approve — approve a payroll run
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "SuperAdmin,FinanceOfficer")]
        public async Task<IActionResult> Approve(int id)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email)
                ?? "system";

            try
            {
                var run = await _payrollService
                    .ApprovePayrollRunAsync(id, currentUserEmail);

                if (run == null)
                    return NotFound(new {
                        message = $"Payroll run with ID {id} not found"
                    });

                return Ok(run);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/payroll/5 — delete a draft payroll run
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,HRAdmin")]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email)
                ?? "system";

            try
            {
                var result = await _payrollService
                    .DeleteDraftPayrollRunAsync(id, currentUserEmail);

                if (!result)
                    return NotFound(new {
                        message = $"Payroll run with ID {id} not found"
                    });

                return Ok(new {
                    message = "Draft payroll run deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}