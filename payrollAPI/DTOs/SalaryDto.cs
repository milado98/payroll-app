using System.ComponentModel.DataAnnotations;

namespace PayrollAPI.DTOs
{
    public class CreateSalaryDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Basic salary must be greater than 0")]
        public decimal BasicSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal HousingAllowance { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal TransportAllowance { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal MealAllowance { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal OtherAllowances { get; set; } = 0;
    }

    public class UpdateSalaryDto
    {
        [Range(1, double.MaxValue)]
        public decimal? BasicSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? HousingAllowance { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? TransportAllowance { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MealAllowance { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? OtherAllowances { get; set; }
    }

    public class SalaryResponseDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
        public decimal HousingAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal MealAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal GrossSalary { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}