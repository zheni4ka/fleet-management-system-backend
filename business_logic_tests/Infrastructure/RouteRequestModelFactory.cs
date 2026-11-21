using business_logic.DTOs;
using business_logic.Entities;

namespace business_logic_tests.Infrastructure
{
    public static class RouteRequestModelFactory
    {
        public const string KyivStartAddressLine = "вулиця Хрещатик, 22";
        public const string LvivDestinationAddressLine = "Городоцька вулиця, 120";

        public static CreateRouteRequestModel CreateRouteRequestModelFor(int autoId, int driverId, DateTime departure, DateTime arrival)
        {
            return new CreateRouteRequestModel
            {
                AutoId = autoId,
                DriverId = driverId,
                StartLocationId = BusinessLogicTestHost.StartLocationId,
                DestinationLocationId = BusinessLogicTestHost.DestinationLocationId,
                StartAddressLine = KyivStartAddressLine,
                StartAddressLatitude = 50.4501,
                StartAddressLongitude = 30.5234,
                DestinationAddressLine = LvivDestinationAddressLine,
                DestinationAddressLatitude = 49.8352691,
                DestinationAddressLongitude = 23.9924747,
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
