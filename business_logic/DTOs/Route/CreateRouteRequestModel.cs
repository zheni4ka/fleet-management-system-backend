using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed record CreateRouteRequestModel : IRouteAddressesRequestModel
    {
        public int StartLocationId { get; init; }
        public int DestinationLocationId { get; init; }
        public string? StartAddressLine { get; init; }
        public double? StartAddressLatitude { get; init; }
        public double? StartAddressLongitude { get; init; }
        public string? DestinationAddressLine { get; init; }
        public double? DestinationAddressLatitude { get; init; }
        public double? DestinationAddressLongitude { get; init; }
        public DateTime DepartureTimeUtc { get; init; }
        public DateTime ArrivalTimeUtc { get; init; }
        public int AutoId { get; init; }
        public int DriverId { get; init; }
        public RouteStatus Status { get; init; } = RouteStatus.Planned;
    }
}
