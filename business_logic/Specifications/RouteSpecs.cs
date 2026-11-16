using Ardalis.Specification;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Helpers;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class RouteSpecs
    {
        private static readonly Expression<Func<Route, GetRouteResponseModel>> GetRouteResponseModelSelector = r => new GetRouteResponseModel
        {
            Id = r.Id,
            StartLocationId = r.StartLocationId,
            StartLocationCity = r.StartLocation.City,
            StartLocationCountry = r.StartLocation.Country,
            StartLocationTimeZoneId = r.StartLocation.TimeZoneId,
            DestinationLocationId = r.DestinationLocationId,
            DestinationLocationCity = r.DestinationLocation.City,
            DestinationLocationCountry = r.DestinationLocation.Country,
            DestinationLocationTimeZoneId = r.DestinationLocation.TimeZoneId,
            DepartureTimeUtc = r.DepartureTimeUtc,
            ArrivalTimeUtc = r.ArrivalTimeUtc,
            AutoId = r.AutoId,
            AutoMark = r.Auto.Mark,
            AutoModel = r.Auto.Model,
            AutoNumber = r.Auto.Number,
            DriverId = r.DriverId,
            DriverName = r.Driver.Name,
            DriverSurname = r.Driver.Surname,
            Status = r.Status,
            DepotId = r.DepotId,
            DepotName = r.Depot!.Name,
            RowVersion = r.RowVersion
        };

        public class ById : Specification<Route>
        {
            public ById(int id)
            {
                Query.Where(r => r.Id == id);
            }
        }

        public class ResponseModelById : Specification<Route, GetRouteResponseModel>
        {
            public ResponseModelById(int id)
            {
                Query.Where(r => r.Id == id);
                Query.Select(GetRouteResponseModelSelector);
            }
        }

        public class All : Specification<Route>
        {
            public All()
            {
                Query.OrderBy(r => r.Id);
            }
        }

        public class Active : Specification<Route>
        {
            public Active()
            {
                Query.Where(r => RouteStatusGroups.Active.Contains(r.Status));
            }
        }

        public class ByDepotId : Specification<Route>
        {
            public ByDepotId(int depotId)
            {
                Query.Where(r => r.DepotId == depotId);
            }
        }

        public class ActiveByAutoId : Specification<Route>
        {
            public ActiveByAutoId(int autoId)
            {
                Query.Where(r => r.AutoId == autoId && RouteStatusGroups.Active.Contains(r.Status));
            }
        }

        public class ActiveByDriverId : Specification<Route>
        {
            public ActiveByDriverId(int driverId)
            {
                Query.Where(r => r.DriverId == driverId && RouteStatusGroups.Active.Contains(r.Status));
            }
        }

        public class ActiveByLocationId : Specification<Route>
        {
            public ActiveByLocationId(int locationId)
            {
                Query.Where(r => (r.StartLocationId == locationId || r.DestinationLocationId == locationId)
                    && RouteStatusGroups.Active.Contains(r.Status));
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
                    && RouteStatusGroups.Active.Contains(r.Status)
                    && r.DepartureTimeUtc >= from);
            }
        }

        public class PagedListBySearchFilter : Specification<Route, GetRouteResponseModel>
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
                        || r.StartLocation.City.Contains(trimmedSearch)
                        || r.StartLocation.Country.Contains(trimmedSearch)
                        || r.DestinationLocation.City.Contains(trimmedSearch)
                        || r.DestinationLocation.Country.Contains(trimmedSearch));
                }

                var sortField = pagedListRequest.Sort ?? RouteSortField.DepartureTime;
                Expression<Func<Route, object?>> sortKeySelector = sortField switch
                {
                    RouteSortField.Id => r => r.Id,
                    RouteSortField.DepartureTime => r => r.DepartureTimeUtc,
                    RouteSortField.ArrivalTime => r => r.ArrivalTimeUtc,
                    RouteSortField.Status => r => r.Status,
                    _ => throw new Exception(string.Format(ExceptionMessages.SortFieldNotSupportedTemplate, sortField))
                };

                Query.OrderBySortDirection(sortKeySelector, r => r.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(GetRouteResponseModelSelector);
            }
        }

        public class InProgressByAutoId : Specification<Route>
        {
            public InProgressByAutoId(int autoId)
            {
                Query.Where(r => r.AutoId == autoId && r.Status == RouteStatus.InProgress);
            }
        }

        public class ByDeparturePeriod : Specification<Route, GetRouteResponseModel>
        {
            public ByDeparturePeriod(DateTime periodStartDateTime, DateTime periodEndExclusiveDateTime)
            {
                Query.Where(r => r.DepartureTimeUtc >= periodStartDateTime && r.DepartureTimeUtc < periodEndExclusiveDateTime)
                    .OrderBy(r => r.DepartureTimeUtc)
                    .ThenBy(r => r.Id);
                Query.Select(GetRouteResponseModelSelector);
            }
        }

        public class ActiveIdOverlappingForAuto : Specification<Route, int?>
        {
            public ActiveIdOverlappingForAuto(int autoId, DateTime departure, DateTime arrival, int? excludedRouteId = null)
            {
                Query.Where(r => r.AutoId == autoId);
                ApplyActiveRouteIdOverlappingIntervalQuery(Query, departure, arrival, excludedRouteId);
            }
        }

        public class ActiveIdOverlappingForDriver : Specification<Route, int?>
        {
            public ActiveIdOverlappingForDriver(int driverId, DateTime departure, DateTime arrival, int? excludedRouteId = null)
            {
                Query.Where(r => r.DriverId == driverId);
                ApplyActiveRouteIdOverlappingIntervalQuery(Query, departure, arrival, excludedRouteId);
            }
        }

        private static void ApplyActiveRouteIdOverlappingIntervalQuery(
            ISpecificationBuilder<Route, int?> routeIdQuery,
            DateTime departure,
            DateTime arrival,
            int? excludedRouteId)
        {
            routeIdQuery.Where(r => RouteStatusGroups.Active.Contains(r.Status)
                && r.DepartureTimeUtc < arrival
                && r.ArrivalTimeUtc > departure);

            if (excludedRouteId.HasValue)
            {
                var excludedRouteIdValue = excludedRouteId.Value;
                routeIdQuery.Where(r => r.Id != excludedRouteIdValue);
            }

            routeIdQuery.OrderBy(r => r.DepartureTimeUtc);
            routeIdQuery.Select(r => (int?)r.Id);
        }

        private static void ApplySearchFilterCriteria(ISpecificationBuilder<Route> routeQuery, GetRoutesBySearchFilterRequestModel searchFilter)
        {
            if (!string.IsNullOrWhiteSpace(searchFilter.StartCity))
            {
                var trimmedStartCity = searchFilter.StartCity.Trim();
                routeQuery.Where(r => r.StartLocation.City.Contains(trimmedStartCity));
            }

            if (!string.IsNullOrWhiteSpace(searchFilter.DestinationCity))
            {
                var trimmedDestinationCity = searchFilter.DestinationCity.Trim();
                routeQuery.Where(r => r.DestinationLocation.City.Contains(trimmedDestinationCity));
            }

            if (searchFilter.DepartureFromDate.HasValue)
            {
                var departureFromDateTime = BusinessTimeZoneConverter.ConvertBusinessDateStartToUtc(searchFilter.DepartureFromDate.Value);
                routeQuery.Where(r => r.DepartureTimeUtc >= departureFromDateTime);
            }

            if (searchFilter.DepartureToDate.HasValue)
            {
                var departureToExclusiveDateTime = BusinessTimeZoneConverter.ConvertBusinessDateStartToUtc(searchFilter.DepartureToDate.Value.AddDays(1));
                routeQuery.Where(r => r.DepartureTimeUtc < departureToExclusiveDateTime);
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

            if (searchFilter.DepotId.HasValue)
            {
                var searchedDepotId = searchFilter.DepotId.Value;
                routeQuery.Where(r => r.DepotId == searchedDepotId);
            }
        }
    }
}
