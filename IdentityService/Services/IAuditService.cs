using IdentityService.Data;
namespace IdentityService.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog logEntry);
    }
}