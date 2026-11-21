using business_logic.Interfaces;

namespace business_logic.Entities
{
    public class DriverTrip : IBelongsToDepot
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public Driver? Driver { get; set; }
        public int? RouteId { get; set; }
        public Route? Route { get; set; }
        public int AutoId { get; set; }
        public string AutoMark { get; set; } = string.Empty;
        public string AutoModel { get; set; } = string.Empty;
        public string AutoNumber { get; set; } = string.Empty;
        public string StartLocationCity { get; set; } = string.Empty;
        public string StartLocationCountry { get; set; } = string.Empty;
        public string StartLocationTimeZoneId { get; set; } = string.Empty;
        public string DestinationLocationCity { get; set; } = string.Empty;
        public string DestinationLocationCountry { get; set; } = string.Empty;
        public string DestinationLocationTimeZoneId { get; set; } = string.Empty;
        public string? StartAddressLine { get; set; }
        public string? DestinationAddressLine { get; set; }
        public DateTime PlannedDepartureTimeUtc { get; set; }
        public DateTime PlannedArrivalTimeUtc { get; set; }
        public DateTime StartedAtUtc { get; set; }
        public DateTime EndedAtUtc { get; set; }
        public int DrivingMinutes { get; set; }
        public RouteStatus FinalStatus { get; set; }
        public int DepotId { get; set; }
        public Depot? Depot { get; set; }
    }
}
