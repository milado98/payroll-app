namespace PayrollAPI.Models
{
    public class Salary
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal HousingAllowance { get; set; }

        public decimal TransportAllowance { get; set; }

        public decimal MealAllowance { get; set; }

        public decimal OtherAllowances { get; set; }

        // Computed property - not stored in DB
        public decimal GrossSalary =>
            BasicSalary + HousingAllowance + TransportAllowance
            + MealAllowance + OtherAllowances;

        public DateTime EffectiveDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public Employee? Employee { get; set; }
    }
}