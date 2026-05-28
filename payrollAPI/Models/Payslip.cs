namespace PayrollAPI.Models
{
    public class Payslip
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int PayrollRunId { get; set; }

        public string PayPeriod { get; set; } = string.Empty;

        public decimal BasicSalary { get; set; }

        public decimal TotalAllowances { get; set; }

        public decimal GrossPay { get; set; }

        // Deductions
        public decimal PAYE { get; set; }        // Income tax

        public decimal Pension { get; set; }     // 8% employee

        public decimal EmployerPension { get; set; } // 10% employer

        public decimal NHF { get; set; }         // 2.5% of basic

        public decimal OtherDeductions { get; set; }

        public decimal TotalDeductions { get; set; }

        public decimal NetPay { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Employee? Employee { get; set; }

        public PayrollRun? PayrollRun { get; set; }
    }
}