namespace business_logic.DTOs
{
    public sealed record AuditLogPropertyChangeModel(string PropertyName, string? PreviousValue, string? CurrentValue);
}
