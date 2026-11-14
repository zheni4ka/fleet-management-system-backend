using Ardalis.Specification;
using business_logic.DTOs;
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

        public class BySearchFilter : Specification<Route>
        {
            public BySearchFilter(GetRoutesBySearchFilterRequestModel searchFilter)
            {
                if (!string.IsNullOrWhiteSpace(searchFilter.StartCity))
                {
                    var trimmedStartCity = searchFilter.StartCity.Trim();
                    Query.Where(r => r.StartLocation != null && r.StartLocation.City.Contains(trimmedStartCity));
                }

                if (!string.IsNullOrWhiteSpace(searchFilter.DestinationCity))
                {
                    var trimmedDestinationCity = searchFilter.DestinationCity.Trim();
                    Query.Where(r => r.DestinationLocation != null && r.DestinationLocation.City.Contains(trimmedDestinationCity));
                }

                if (searchFilter.DepartureFromDate.HasValue)
                {
                    var departureFromDateTime = searchFilter.DepartureFromDate.Value.ToDateTime(TimeOnly.MinValue);
                    Query.Where(r => r.DepartureTime >= departureFromDateTime);
                }

                if (searchFilter.DepartureToDate.HasValue)
                {
                    var departureToExclusiveDateTime = searchFilter.DepartureToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                    Query.Where(r => r.DepartureTime < departureToExclusiveDateTime);
                }

                if (searchFilter.Status.HasValue)
                {
                    var searchedRouteStatus = searchFilter.Status.Value;
                    Query.Where(r => r.Status == searchedRouteStatus);
                }

                if (searchFilter.DriverId.HasValue)
                {
                    var searchedDriverId = searchFilter.DriverId.Value;
                    Query.Where(r => r.DriverId == searchedDriverId);
                }

                if (searchFilter.AutoId.HasValue)
                {
                    var searchedAutoId = searchFilter.AutoId.Value;
                    Query.Where(r => r.AutoId == searchedAutoId);
                }

                Query.OrderBy(r => r.DepartureTime);
            }
        }

        public class ByDeparturePeriod : Specification<Route>
        {
            public ByDeparturePeriod(DateTime periodStartDateTime, DateTime periodEndExclusiveDateTime)
            {
                Query.Where(r => r.DepartureTime >= periodStartDateTime && r.DepartureTime < periodEndExclusiveDateTime)
                    .OrderBy(r => r.DepartureTime);
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
