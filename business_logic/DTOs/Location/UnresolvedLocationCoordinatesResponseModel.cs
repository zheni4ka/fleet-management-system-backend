namespace business_logic.DTOs
{
    public sealed record UnresolvedLocationCoordinatesResponseModel(
        int LocationId,
        string City,
        string Country,
        UnresolvedLocationCoordinatesReason Reason);
}
