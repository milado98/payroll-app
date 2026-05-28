using System.ComponentModel.DataAnnotations;

namespace PayrollAPI.DTOs
{
    public class CreatePayrollRunDto
    {
        [Required(ErrorMessage = "Pay period is required")]
        // Format must be YYYY-MM e.g. "2024-01"
        [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$",
            ErrorMessage = "Pay period must be in format YYYY-MM e.g. 2024-01")]
        public string PayPeriod { get; set; } = string.Empty;
    }

    public class PayrollRunResponseDto
    {
        public int Id { get; set; }
        public string PayPeriod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetPay { get; set; }
        public int TotalEmployees { get; set; }
        public List<PayslipSummaryDto> Payslips { get; set; } = new();
    }

    public class PayslipSummaryDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal GrossPay { get; set; }
        public decimal EmployeePension { get; set; }
        public decimal EmployerPension { get; set; }
        public decimal NHF { get; set; }
        public decimal PAYE { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
    }
}