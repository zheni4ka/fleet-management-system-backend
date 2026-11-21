using System.Text.Json.Serialization;

namespace transport_logistic_management_2026.ExternalModels
{
    public sealed record PhotonFeatureCollection
    {
        [JsonPropertyName("features")]
        public IReadOnlyList<PhotonFeature> Features { get; init; } = [];
    }
}
