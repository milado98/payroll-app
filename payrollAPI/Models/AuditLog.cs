namespace PayrollAPI.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        // What they did e.g. "RAN_PAYROLL", "EDITED_EMPLOYEE"
        public string Action { get; set; } = string.Empty;

        // Which table was affected
        public string Entity { get; set; } = string.Empty;

        public string? Details { get; set; }

        public string IpAddress { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}