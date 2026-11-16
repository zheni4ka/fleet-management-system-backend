using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed record GetAutosBySearchFilterRequestModel(
        AutoStatus? Status,
        string? Color,
        bool IsArchived,
        int? DepotId);
}
