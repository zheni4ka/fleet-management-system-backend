using Ardalis.Specification;
using business_logic.DTOs;
using business_logic.Entities;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class RouteSpecs
    {
        private static readonly Expression<Func<Route, RouteDTO>> RouteDTOSelector = r => new RouteDTO
        {
            Id = r.Id,
            StartLocationId = r.StartLocationId,
            DestinationLocationId = r.DestinationLocationId,
            DepartureTime = r.DepartureTime,
            ArrivalTime = r.ArrivalTime,
            AutoId = r.AutoId,
            DriverId = r.DriverId,
            Status = r.Status
        };

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
                ApplySearchFilterCriteria(Query, searchFilter);
                Query.OrderBy(r => r.DepartureTime);
            }
        }

        public class PagedListBySearchFilter : Specification<Route, RouteDTO>
        {
            public PagedListBySearchFilter(
                GetRoutesBySearchFilterRequestModel searchFilter,
                GetPagedListRequestModel<RouteSortField> pagedListRequest)
            {
                ApplySearchFilterCriteria(Query, searchFilter);

                if (!string.IsNullOrWhiteSpace(pagedListRequest.Search))
                {
                    var trimmedSearch = pagedListRequest.Search.Trim();
                    var searchedId = SearchQueryIdParser.ParseSearchedId(trimmedSearch);
                    Query.Where(r => (searchedId != null && r.Id == searchedId)
                        || (r.StartLocation != null
                            && (r.StartLocation.City.Contains(trimmedSearch) || r.StartLocation.Country.Contains(trimmedSearch)))
                        || (r.DestinationLocation != null
                            && (r.DestinationLocation.City.Contains(trimmedSearch) || r.DestinationLocation.Country.Contains(trimmedSearch))));
                }

                var sortField = pagedListRequest.Sort ?? RouteSortField.DepartureTime;
                Expression<Func<Route, object?>> sortKeySelector = sortField switch
                {
                    RouteSortField.Id => r => r.Id,
                    RouteSortField.DepartureTime => r => r.DepartureTime,
                    RouteSortField.ArrivalTime => r => r.ArrivalTime,
                    RouteSortField.Status => r => r.Status,
                    _ => throw new Exception($"Route sort field '{sortField}' is not supported.")
                };

                Query.OrderBySortDirection(sortKeySelector, r => r.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(RouteDTOSelector);
            }
        }

        public class InProgressByAutoId : Specification<Route>
        {
            public InProgressByAutoId(int autoId)
            {
                Query.Where(r => r.AutoId == autoId && r.Status == RouteStatus.InProgress);
            }
        }

        private static void ApplySearchFilterCriteria(ISpecificationBuilder<Route> routeQuery, GetRoutesBySearchFilterRequestModel searchFilter)
        {
            if (!string.IsNullOrWhiteSpace(searchFilter.StartCity))
            {
                var trimmedStartCity = searchFilter.StartCity.Trim();
                routeQuery.Where(r => r.StartLocation != null && r.StartLocation.City.Contains(trimmedStartCity));
            }

            if (!string.IsNullOrWhiteSpace(searchFilter.DestinationCity))
            {
                var trimmedDestinationCity = searchFilter.DestinationCity.Trim();
                routeQuery.Where(r => r.DestinationLocation != null && r.DestinationLocation.City.Contains(trimmedDestinationCity));
            }

            if (searchFilter.DepartureFromDate.HasValue)
            {
                var departureFromDateTime = searchFilter.DepartureFromDate.Value.ToDateTime(TimeOnly.MinValue);
                routeQuery.Where(r => r.DepartureTime >= departureFromDateTime);
            }

            if (searchFilter.DepartureToDate.HasValue)
            {
                var departureToExclusiveDateTime = searchFilter.DepartureToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
                routeQuery.Where(r => r.DepartureTime < departureToExclusiveDateTime);
            }

            if (searchFilter.Status.HasValue)
            {
                var searchedRouteStatus = searchFilter.Status.Value;
                routeQuery.Where(r => r.Status == searchedRouteStatus);
            }

            if (searchFilter.DriverId.HasValue)
            {
                var searchedDriverId = searchFilter.DriverId.Value;
                routeQuery.Where(r => r.DriverId == searchedDriverId);
            }

            if (searchFilter.AutoId.HasValue)
            {
                var searchedAutoId = searchFilter.AutoId.Value;
                routeQuery.Where(r => r.AutoId == searchedAutoId);
            }
        }

        public class ByDeparturePeriod : Specification<Route, RouteDTO>
        {
            public ByDeparturePeriod(DateTime periodStartDateTime, DateTime periodEndExclusiveDateTime)
            {
                Query.Where(r => r.DepartureTime >= periodStartDateTime && r.DepartureTime < periodEndExclusiveDateTime)
                    .OrderBy(r => r.DepartureTime)
                    .ThenBy(r => r.Id);
                Query.Select(RouteDTOSelector);
            }
        }

        public class ActiveIdOverlappingForAuto : Specification<Route, int?>
        {
            public ActiveIdOverlappingForAuto(int autoId, DateTime departure, DateTime arrival, int? excludeRouteId = null)
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

                Query.OrderBy(r => r.DepartureTime);
                Query.Select(r => (int?)r.Id);
            }
        }

        public class ActiveIdOverlappingForDriver : Specification<Route, int?>
        {
            public ActiveIdOverlappingForDriver(int driverId, DateTime departure, DateTime arrival, int? excludeRouteId = null)
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

                Query.OrderBy(r => r.DepartureTime);
                Query.Select(r => (int?)r.Id);
            }
        }
    }
}
