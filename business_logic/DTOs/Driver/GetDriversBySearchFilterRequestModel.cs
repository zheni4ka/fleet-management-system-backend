namespace business_logic.DTOs
{
    public sealed record GetDriversBySearchFilterRequestModel(
        bool IsArchived,
        int? DepotId);
}
