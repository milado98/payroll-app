using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayrollAPI.DTOs;
using PayrollAPI.Services;
using System.Security.Claims;

namespace PayrollAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ALL endpoints require login
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET /api/employee — get all employees
        // Only HR, Finance, and SuperAdmin can see all employees
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,HRAdmin,FinanceOfficer,Manager")]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        // GET /api/employee/5 — get one employee by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,FinanceOfficer,Manager")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} not found" });

            return Ok(employee);
        }

        // POST /api/employee — create new employee
        // Only HR and SuperAdmin can create employees
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,HRAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            // Check if model validation passed
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check for duplicate email
            if (await _employeeService.EmailExistsAsync(dto.Email))
                return BadRequest(new { 
                    message = "An employee with this email already exists" 
                });

            // Get the email of who is making this request
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "system";

            var employee = await _employeeService
                .CreateEmployeeAsync(dto, currentUserEmail);

            // Return 201 Created with the new employee data
            return CreatedAtAction(
                nameof(GetById), 
                new { id = employee.Id }, 
                employee);
        }

        // PUT /api/employee/5 — update employee
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,HRAdmin")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "system";

            var employee = await _employeeService
                .UpdateEmployeeAsync(id, dto, currentUserEmail);

            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} not found" });

            return Ok(employee);
        }

        // DELETE /api/employee/5 — deactivate employee (NOT a real delete)
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,HRAdmin")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "system";

            var result = await _employeeService
                .DeactivateEmployeeAsync(id, currentUserEmail);

            if (!result)
                return NotFound(new { message = $"Employee with ID {id} not found" });

            return Ok(new { message = "Employee deactivated successfully" });
        }

        // PATCH /api/employee/5/activate — reactivate a deactivated employee
        [HttpPatch("{id}/activate")]
        [Authorize(Roles = "SuperAdmin,HRAdmin")]
        public async Task<IActionResult> Activate(int id)
        {
            var currentUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? "system";

            var result = await _employeeService
                .ActivateEmployeeAsync(id, currentUserEmail);

            if (!result)
                return NotFound(new { message = $"Employee with ID {id} not found" });

            return Ok(new { message = "Employee reactivated successfully" });
        }
    }
}