namespace business_logic.DTOs
{
    public sealed record FilledLocationCoordinatesResponseModel(
        int LocationId,
        string City,
        string Country,
        int GeoNamesId,
        string MatchedCity,
        string? MatchedRegion,
        double Latitude,
        double Longitude,
        string TimeZoneId,
        string PreviousTimeZoneId,
        int OtherCandidatesInCountryCount);
}
