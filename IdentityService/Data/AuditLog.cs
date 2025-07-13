namespace IdentityService.Data
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EventType { get; set; } // örn: "UserLogin", "PasswordChange", "FailedLogin"
        public string? Username { get; set; }
        public string? Details { get; set; } // Ek detaylar (örn: IP adresi)
    }
}