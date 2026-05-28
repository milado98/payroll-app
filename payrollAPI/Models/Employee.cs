using System.ComponentModel.DataAnnotations;

namespace PayrollAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        // FULL-TIME, PART-TIME, CONTRACT
        public string EmploymentType { get; set; } = "FULL-TIME";

        public DateTime StartDate { get; set; }

        // We never delete employees - we deactivate them
        public bool IsActive { get; set; } = true;

        public string BankName { get; set; } = string.Empty;

        public string AccountNumber { get; set; } = string.Empty;

        public string TaxId { get; set; } = string.Empty;

        public string PensionId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property - links to salary
        public Salary? Salary { get; set; }
    }
}

