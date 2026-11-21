namespace business_logic.DTOs
{
    public sealed record GetCityGeocodingCandidateResponseModel
    {
        public int GeoNamesId { get; init; }
        public string City { get; init; } = string.Empty;
        public string? Region { get; init; }
        public string Country { get; init; } = string.Empty;
        public string CountryCode { get; init; } = string.Empty;
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public string TimeZoneId { get; init; } = string.Empty;
        public int? Population { get; init; }
    }
}
