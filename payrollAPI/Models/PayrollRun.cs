namespace PayrollAPI.Models
{
    public class PayrollRun
    {
        public int Id { get; set; }

        // e.g., "2024-01" for January 2024
        public string PayPeriod { get; set; } = string.Empty;

        // DRAFT, PENDING_APPROVAL, APPROVED, PAID
        public string Status { get; set; } = "DRAFT";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public string? ApprovedBy { get; set; }

        public decimal TotalGrossPay { get; set; }

        public decimal TotalDeductions { get; set; }

        public decimal TotalNetPay { get; set; }

        // One payroll run has many payslips
        public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();

    }
}
