using Microsoft.AspNetCore.Identity;

namespace business_logic.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string? DispatcherId { get; set; }
        public IdentityUser? Dispatcher { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public AuditAction Action { get; set; }
        public string? EntityId { get; set; }
    }
}
