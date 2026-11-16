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
        public DateTime DepartureTimeUtc { get; set; }
        public DateTime ArrivalTimeUtc { get; set; }
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
