using System.Text.Json.Serialization;

namespace transport_logistic_management_2026.ExternalModels
{
    public sealed record OpenMeteoGeocodingSearchResponse
    {
        [JsonPropertyName("results")]
        public IReadOnlyList<OpenMeteoGeocodingResult>? Results { get; init; }
    }
}
