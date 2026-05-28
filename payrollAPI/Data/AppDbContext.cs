using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PayrollAPI.Models;

namespace PayrollAPI.Data
{
    // AppUser is our custom user (extends Identity's default user)
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        // Links to employee if this user is an employee
        public int? EmployeeId { get; set; }
    }

    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Each DbSet = one table in the database
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<PayrollRun> PayrollRuns { get; set; }
        public DbSet<Payslip> Payslips { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure PayPeriod is unique per payroll run
            builder.Entity<PayrollRun>()
                .HasIndex(p => p.PayPeriod)
                .IsUnique();

            // Decimal precision for money fields
            builder.Entity<Salary>()
                .Property(s => s.BasicSalary)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Payslip>()
                .Property(p => p.NetPay)
                .HasColumnType("decimal(18,2)");
        }
    }
}