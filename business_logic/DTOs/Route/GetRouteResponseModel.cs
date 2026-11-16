using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed record GetRouteResponseModel
    {
        public int Id { get; init; }
        public int StartLocationId { get; init; }
        public string StartLocationCity { get; init; }
        public string StartLocationCountry { get; init; }
        public int DestinationLocationId { get; init; }
        public string DestinationLocationCity { get; init; }
        public string DestinationLocationCountry { get; init; }
        public DateTime DepartureTime { get; init; }
        public DateTime ArrivalTime { get; init; }
        public int AutoId { get; init; }
        public string AutoMark { get; init; }
        public string AutoModel { get; init; }
        public string AutoNumber { get; init; }
        public int DriverId { get; init; }
        public string DriverName { get; init; }
        public string DriverSurname { get; init; }
        public RouteStatus Status { get; init; }
    }
}
