using System.Text.Json.Serialization;

namespace transport_logistic_management_2026.ExternalModels
{
    public sealed record PhotonFeature
    {
        [JsonPropertyName("geometry")]
        public PhotonPointGeometry? Geometry { get; init; }

        [JsonPropertyName("properties")]
        public PhotonFeatureProperties? Properties { get; init; }
    }
}
