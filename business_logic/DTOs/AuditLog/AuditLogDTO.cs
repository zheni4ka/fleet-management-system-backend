using business_logic.Entities;

namespace business_logic.DTOs
{
    public class AuditLogDTO
    {
        public int Id { get; set; }
        public string? DispatcherId { get; set; }
        public DateTime Time { get; set; }
        public AuditAction Action { get; set; }
        public string? EntityId { get; set; }
    }
}
