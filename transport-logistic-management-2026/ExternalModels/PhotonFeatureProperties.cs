using System.Text.Json.Serialization;

namespace transport_logistic_management_2026.ExternalModels
{
    public sealed record PhotonFeatureProperties
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("street")]
        public string? Street { get; init; }

        [JsonPropertyName("housenumber")]
        public string? HouseNumber { get; init; }

        [JsonPropertyName("district")]
        public string? District { get; init; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; init; }
    }
}
