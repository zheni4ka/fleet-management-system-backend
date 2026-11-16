using business_logic.Entities;
namespace business_logic.DTOs
{
    public class RouteDTO
    {
        public int Id { get; set; }
        public int StartLocationId { get; set; }
        public int DestinationLocationId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int AutoId { get; set; }
        public int DriverId { get; set; }
        public RouteStatus Status { get; set; }
    }
}
