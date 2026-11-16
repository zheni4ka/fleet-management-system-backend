using business_logic.DTOs;
using business_logic.Entities;

namespace business_logic_tests.Infrastructure
{
    public static class RouteRequestModelFactory
    {
        public static CreateRouteRequestModel CreateRouteRequestModelFor(int autoId, int driverId, DateTime departure, DateTime arrival)
        {
            return new CreateRouteRequestModel
            {
                AutoId = autoId,
                DriverId = driverId,
                StartLocationId = BusinessLogicTestHost.StartLocationId,
                DestinationLocationId = BusinessLogicTestHost.DestinationLocationId,
                DepartureTimeUtc = DateTime.SpecifyKind(departure, DateTimeKind.Utc),
                ArrivalTimeUtc = DateTime.SpecifyKind(arrival, DateTimeKind.Utc),
                Status = RouteStatus.Planned
            };
        }

        public static UpdateRouteRequestModel CreateUpdateRouteRequestModelFor(Route route, int autoId, int driverId, RouteStatus status)
        {
            return new UpdateRouteRequestModel
            {
                Id = route.Id,
                RowVersion = route.RowVersion,
                AutoId = autoId,
                DriverId = driverId,
                StartLocationId = BusinessLogicTestHost.StartLocationId,
                DestinationLocationId = BusinessLogicTestHost.DestinationLocationId,
                Status = status
            };
        }
    }
}
