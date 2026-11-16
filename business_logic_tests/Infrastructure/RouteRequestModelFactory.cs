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
                DepartureTime = departure,
                ArrivalTime = arrival,
                Status = RouteStatus.Planned
            };
        }

        public static UpdateRouteRequestModel CreateUpdateRouteRequestModelFor(int routeId, int autoId, int driverId, RouteStatus status)
        {
            return new UpdateRouteRequestModel
            {
                Id = routeId,
                AutoId = autoId,
                DriverId = driverId,
                StartLocationId = BusinessLogicTestHost.StartLocationId,
                DestinationLocationId = BusinessLogicTestHost.DestinationLocationId,
                Status = status
            };
        }
    }
}
