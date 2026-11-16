using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed class AuditLogPageRowModel
    {
        public int Id { get; init; }
        public string? DispatcherId { get; init; }
        public string? DispatcherUsername { get; init; }
        public DateTime Time { get; init; }
        public AuditAction Action { get; init; }
        public string? EntityId { get; init; }
        public string? ChangesJson { get; init; }
    }
}
