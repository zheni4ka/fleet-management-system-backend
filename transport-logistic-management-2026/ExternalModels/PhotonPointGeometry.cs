using System.Text.Json.Serialization;

namespace transport_logistic_management_2026.ExternalModels
{
    public sealed record PhotonPointGeometry
    {
        [JsonPropertyName("coordinates")]
        public IReadOnlyList<double> Coordinates { get; init; } = [];
    }
}
