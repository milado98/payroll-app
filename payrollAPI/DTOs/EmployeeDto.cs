using System.ComponentModel.DataAnnotations;

namespace PayrollAPI.DTOs
{
    // Used when CREATING a new employee
    public class CreateEmployeeDto
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress(ErrorMessage = "Enter a valid email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{11}$", 
            ErrorMessage = "Phone number must be 11 digits")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        // FULL-TIME, PART-TIME, CONTRACT
        public string EmploymentType { get; set; } = "FULL-TIME";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{10}$", 
            ErrorMessage = "Account number must be 10 digits")]
        public string AccountNumber { get; set; } = string.Empty;

        public string TaxId { get; set; } = string.Empty;

        public string PensionId { get; set; } = string.Empty;
    }

    // Used when UPDATING an employee
    public class UpdateEmployeeDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? EmploymentType { get; set; }
        public string? BankName { get; set; }

        [RegularExpression(@"^\d{10}$", 
            ErrorMessage = "Account number must be 10 digits")]
        public string? AccountNumber { get; set; }
        public string? TaxId { get; set; }
        public string? PensionId { get; set; }
    }

    // Used when RETURNING employee data to the frontend
    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public bool IsActive { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string PensionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Salary info if available
        public decimal? BasicSalary { get; set; }
        public decimal? GrossSalary { get; set; }
    }
}