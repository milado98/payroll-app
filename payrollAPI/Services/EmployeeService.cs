using Microsoft.EntityFrameworkCore;
using PayrollAPI.Data;
using PayrollAPI.DTOs;
using PayrollAPI.Models;

namespace PayrollAPI.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeResponseDto>> GetAllEmployeesAsync();
        Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeResponseDto> CreateEmployeeAsync(
            CreateEmployeeDto dto, string createdByEmail);
        Task<EmployeeResponseDto?> UpdateEmployeeAsync(
            int id, UpdateEmployeeDto dto, string updatedByEmail);
        Task<bool> DeactivateEmployeeAsync(
            int id, string deactivatedByEmail);
        Task<bool> ActivateEmployeeAsync(
            int id, string activatedByEmail);
        Task<bool> EmailExistsAsync(string email);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;

        public EmployeeService(AppDbContext context)
        {
            _context = context;
        }

        // GET ALL EMPLOYEES
        public async Task<List<EmployeeResponseDto>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                .Include(e => e.Salary)
                .Where(e => e.IsActive)
                .Select(e => MapToResponseDto(e))
                .ToListAsync();
        }

        // GET ONE EMPLOYEE BY ID
        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Salary)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return null;

            return MapToResponseDto(employee);
        }

        // CREATE NEW EMPLOYEE
        public async Task<EmployeeResponseDto> CreateEmployeeAsync(
            CreateEmployeeDto dto, string createdByEmail)
        {
            var employee = new Employee
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.ToLower().Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                JobTitle = dto.JobTitle.Trim(),
                Department = dto.Department.Trim(),
                EmploymentType = dto.EmploymentType,
                StartDate = dto.StartDate,
                BankName = dto.BankName.Trim(),
                AccountNumber = dto.AccountNumber.Trim(),
                TaxId = dto.TaxId.Trim(),
                PensionId = dto.PensionId.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Log the action
            await LogAudit(createdByEmail, "CREATED_EMPLOYEE",
                "Employee", $"Created employee: {employee.FirstName} {employee.LastName}");

            return MapToResponseDto(employee);
        }

        // UPDATE EMPLOYEE
        public async Task<EmployeeResponseDto?> UpdateEmployeeAsync(
            int id, UpdateEmployeeDto dto, string updatedByEmail)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return null;

            // Only update fields that were actually sent
            if (!string.IsNullOrEmpty(dto.FirstName))
                employee.FirstName = dto.FirstName.Trim();
            if (!string.IsNullOrEmpty(dto.LastName))
                employee.LastName = dto.LastName.Trim();
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                employee.PhoneNumber = dto.PhoneNumber.Trim();
            if (!string.IsNullOrEmpty(dto.JobTitle))
                employee.JobTitle = dto.JobTitle.Trim();
            if (!string.IsNullOrEmpty(dto.Department))
                employee.Department = dto.Department.Trim();
            if (!string.IsNullOrEmpty(dto.EmploymentType))
                employee.EmploymentType = dto.EmploymentType;
            if (!string.IsNullOrEmpty(dto.BankName))
                employee.BankName = dto.BankName.Trim();
            if (!string.IsNullOrEmpty(dto.AccountNumber))
                employee.AccountNumber = dto.AccountNumber.Trim();
            if (!string.IsNullOrEmpty(dto.TaxId))
                employee.TaxId = dto.TaxId.Trim();
            if (!string.IsNullOrEmpty(dto.PensionId))
                employee.PensionId = dto.PensionId.Trim();

            await _context.SaveChangesAsync();

            await LogAudit(updatedByEmail, "UPDATED_EMPLOYEE",
                "Employee", $"Updated employee ID: {id}");

            return MapToResponseDto(employee);
        }

        // DEACTIVATE (soft delete - never hard delete)
        public async Task<bool> DeactivateEmployeeAsync(
            int id, string deactivatedByEmail)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return false;

            employee.IsActive = false;
            await _context.SaveChangesAsync();

            await LogAudit(deactivatedByEmail, "DEACTIVATED_EMPLOYEE",
                "Employee", $"Deactivated employee ID: {id}");

            return true;
        }

        // REACTIVATE AN EMPLOYEE
        public async Task<bool> ActivateEmployeeAsync(
            int id, string activatedByEmail)
        {
            var employee = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return false;

            employee.IsActive = true;
            await _context.SaveChangesAsync();

            await LogAudit(activatedByEmail, "ACTIVATED_EMPLOYEE",
                "Employee", $"Reactivated employee ID: {id}");

            return true;
        }

        // CHECK IF EMAIL ALREADY EXISTS
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Employees
                .AnyAsync(e => e.Email == email.ToLower().Trim());
        }

        // AUDIT LOGGER - records every action
        private async Task LogAudit(
            string userEmail, string action, 
            string entity, string details)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserEmail = userEmail,
                UserId = userEmail,
                Action = action,
                Entity = entity,
                Details = details,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        // MAP Employee model to EmployeeResponseDto
        private static EmployeeResponseDto MapToResponseDto(Employee e)
        {
            return new EmployeeResponseDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                JobTitle = e.JobTitle,
                Department = e.Department,
                EmploymentType = e.EmploymentType,
                StartDate = e.StartDate,
                IsActive = e.IsActive,
                BankName = e.BankName,
                AccountNumber = e.AccountNumber,
                TaxId = e.TaxId,
                PensionId = e.PensionId,
                CreatedAt = e.CreatedAt,
                BasicSalary = e.Salary?.BasicSalary,
                GrossSalary = e.Salary?.GrossSalary
            };
        }
    }
}