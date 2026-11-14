using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed record GetRoutesBySearchFilterRequestModel(
        string? StartCity,
        string? DestinationCity,
        DateOnly? DepartureFromDate,
        DateOnly? DepartureToDate,
        RouteStatus? Status,
        int? DriverId,
        int? AutoId);
}
