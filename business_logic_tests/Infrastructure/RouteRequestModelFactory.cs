using business_logic.DTOs;
using business_logic.Entities;

namespace business_logic_tests.Infrastructure
{
    public static class RouteRequestModelFactory
    {
        public static CreateRouteModel CreateRouteModelFor(int autoId, int driverId, DateTime departure, DateTime arrival)
        {
            return new CreateRouteModel
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

        public static EditRouteModel CreateEditRouteModelFor(int routeId, int autoId, int driverId, RouteStatus status)
        {
            return new EditRouteModel
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
