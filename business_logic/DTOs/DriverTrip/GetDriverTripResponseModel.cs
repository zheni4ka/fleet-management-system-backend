using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed record GetDriverTripResponseModel
    {
        public int Id { get; init; }
        public int DriverId { get; init; }
        public int? RouteId { get; init; }
        public int AutoId { get; init; }
        public string AutoMark { get; init; } = string.Empty;
        public string AutoModel { get; init; } = string.Empty;
        public string AutoNumber { get; init; } = string.Empty;
        public string StartLocationCity { get; init; } = string.Empty;
        public string StartLocationCountry { get; init; } = string.Empty;
        public string StartLocationTimeZoneId { get; init; } = string.Empty;
        public string DestinationLocationCity { get; init; } = string.Empty;
        public string DestinationLocationCountry { get; init; } = string.Empty;
        public string DestinationLocationTimeZoneId { get; init; } = string.Empty;
        public string? StartAddressLine { get; init; }
        public string? DestinationAddressLine { get; init; }
        public DateTime PlannedDepartureTimeUtc { get; init; }
        public DateTime PlannedArrivalTimeUtc { get; init; }
        public DateTime StartedAtUtc { get; init; }
        public DateTime EndedAtUtc { get; init; }
        public int DrivingMinutes { get; init; }
        public RouteStatus FinalStatus { get; init; }
    }
}
