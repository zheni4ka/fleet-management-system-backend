

namespace business_logic.Entities
{
    public enum RouteStatus
    {
        Planned = 0,
        InProgress,
        Completed,
        Cancelled
    }

    public class Route
    {
        public int Id { get; set; }

        public int StartLocationId { get; set; }
        public Location StartLocation { get; set; }
        public int DestinationLocationId { get; set; }
        public Location DestinationLocation { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public RouteStatus Status { get; set; } = RouteStatus.Planned;
        public Auto Auto { get; set; }
        public int AutoId { get; set; }  
        public Driver Driver { get; set; }
        public int DriverId { get; set; }
    }
}
