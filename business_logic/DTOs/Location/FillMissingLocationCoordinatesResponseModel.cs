namespace business_logic.DTOs
{
    public sealed record FillMissingLocationCoordinatesResponseModel(
        IReadOnlyList<FilledLocationCoordinatesResponseModel> FilledLocations,
        IReadOnlyList<UnresolvedLocationCoordinatesResponseModel> UnresolvedLocations);
}
