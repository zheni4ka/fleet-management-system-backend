using Ardalis.Specification;
using business_logic.Entities;

namespace business_logic.Specifications
{
    public class RouteSpecs
    {
        public class ById : Specification<Route>
        {
            public ById(int id)
            {
                Query.Where(r => r.Id == id);
            }
        }

        public class All : Specification<Route>
        {
            public All()
            {
                Query.OrderBy(r => r.Id);
            }
        }

        public class ByStart : Specification<Route>
        {
            public ByStart(string city)
            {
                Query.Where(r => r.StartLocation != null && r.StartLocation.City == city);
            }
        }

        public class ByDestination : Specification<Route>
        {
            public ByDestination(string city)
            {
                Query.Where(r => r.DestinationLocation != null && r.DestinationLocation.City == city);
            }
        }

        public class ByStartDate : Specification<Route>
        {
            public ByStartDate(DateTime dateTime)
            {
                Query.Where(r => r.DepartureTime == dateTime);
            }
        }

        public class ByEndDate : Specification<Route>
        {
            public ByEndDate(DateTime dateTime)
            {
                Query.Where(r => r.ArrivalTime == dateTime);
            }
        }

        public class ByDriverId : Specification<Route>
        {
            public ByDriverId(int driverId)
            {
                Query.Where(r => r.DriverId == driverId);
            }
        }

        public class ByAutoId : Specification<Route>
        {
            public ByAutoId(int autoId)
            {
                Query.Where(r => r.AutoId == autoId);
            }
        }

        public class ByLocationId : Specification<Route>
        {
            public ByLocationId(int locationId)
            {
                Query.Where(r => r.StartLocationId == locationId || r.DestinationLocationId == locationId);
            }
        }

        public class ActiveByAutoFrom : Specification<Route>
        {
            public ActiveByAutoFrom(int autoId, DateTime from)
            {
                Query.Where(r => r.AutoId == autoId
                    && (r.Status == RouteStatus.Planned || r.Status == RouteStatus.InProgress)
                    && r.DepartureTime >= from);
            }
        }

        public class OverlappingForAuto : Specification<Route>
        {
            public OverlappingForAuto(int autoId, DateTime departure, DateTime arrival, int? excludeRouteId = null)
            {
                Query.Where(r => r.AutoId == autoId
                    && r.Status != RouteStatus.Cancelled
                    && r.Status != RouteStatus.Completed
                    && r.DepartureTime < arrival
                    && r.ArrivalTime > departure);

                if (excludeRouteId.HasValue)
                {
                    Query.Where(r => r.Id != excludeRouteId.Value);
                }
            }
        }

        public class OverlappingForDriver : Specification<Route>
        {
            public OverlappingForDriver(int driverId, DateTime departure, DateTime arrival, int? excludeRouteId = null)
            {
                Query.Where(r => r.DriverId == driverId
                    && r.Status != RouteStatus.Cancelled
                    && r.Status != RouteStatus.Completed
                    && r.DepartureTime < arrival
                    && r.ArrivalTime > departure);

                if (excludeRouteId.HasValue)
                {
                    Query.Where(r => r.Id != excludeRouteId.Value);
                }
            }
        }
    }
}
