using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed record UpdateRouteRequestModel
    {
        public int Id { get; init; }
        public int StartLocationId { get; init; }
        public int DestinationLocationId { get; init; }
        public int AutoId { get; init; }
        public int DriverId { get; init; }
        public DateTime? DepartureTimeUtc { get; init; }
        public DateTime? ArrivalTimeUtc { get; init; }
        public RouteStatus Status { get; init; }
        public byte[] RowVersion { get; init; } = [];
    }
}
