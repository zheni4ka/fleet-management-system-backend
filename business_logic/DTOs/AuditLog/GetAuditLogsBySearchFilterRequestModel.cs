using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed record GetAuditLogsBySearchFilterRequestModel(
        AuditAction? Action,
        DateOnly? TimeFromDate,
        DateOnly? TimeToDate);
}
