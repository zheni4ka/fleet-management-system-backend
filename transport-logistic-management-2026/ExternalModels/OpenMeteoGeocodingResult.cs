using System.Text.Json.Serialization;

namespace transport_logistic_management_2026.ExternalModels
{
    public sealed record OpenMeteoGeocodingResult
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Latitude { get; init; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; init; }

        [JsonPropertyName("country")]
        public string? Country { get; init; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; init; }

        [JsonPropertyName("admin1")]
        public string? FirstLevelAdministrativeRegion { get; init; }

        [JsonPropertyName("timezone")]
        public string? TimeZoneId { get; init; }

        [JsonPropertyName("population")]
        public int? Population { get; init; }
    }
}
