namespace business_logic.DTOs
{
    public sealed record GetAddressCandidatesRequestModel(
        string? SearchText,
        double? Latitude,
        double? Longitude);
}
