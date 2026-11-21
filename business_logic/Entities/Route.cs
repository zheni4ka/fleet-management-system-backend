using business_logic.Interfaces;

namespace business_logic.Entities
{
    public class Route : IHasRowVersion, IBelongsToDepot
    {
        public int Id { get; set; }

        public int StartLocationId { get; set; }
        public Location StartLocation { get; set; }
        public int DestinationLocationId { get; set; }
        public Location DestinationLocation { get; set; }
        public string? StartAddressLine { get; set; }
        public double? StartAddressLatitude { get; set; }
        public double? StartAddressLongitude { get; set; }
        public string? DestinationAddressLine { get; set; }
        public double? DestinationAddressLatitude { get; set; }
        public double? DestinationAddressLongitude { get; set; }
        public DateTime DepartureTimeUtc { get; set; }
        public DateTime ArrivalTimeUtc { get; set; }
        public DateTime? ActualDepartureTimeUtc { get; set; }
        public DateTime? ActualArrivalTimeUtc { get; set; }
        public RouteStatus Status { get; set; } = RouteStatus.Planned;
        public Auto Auto { get; set; }
        public int AutoId { get; set; }
        public Driver Driver { get; set; }
        public int DriverId { get; set; }
        public int DepotId { get; set; }
        public Depot? Depot { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
