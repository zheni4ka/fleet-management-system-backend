using Ardalis.Specification;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class DriverTripSpecs
    {
        private static readonly Expression<Func<DriverTrip, GetDriverTripResponseModel>> GetDriverTripResponseModelSelector = t => new GetDriverTripResponseModel
        {
            Id = t.Id,
            DriverId = t.DriverId,
            RouteId = t.RouteId,
            AutoId = t.AutoId,
            AutoMark = t.AutoMark,
            AutoModel = t.AutoModel,
            AutoNumber = t.AutoNumber,
            StartLocationCity = t.StartLocationCity,
            StartLocationCountry = t.StartLocationCountry,
            StartLocationTimeZoneId = t.StartLocationTimeZoneId,
            DestinationLocationCity = t.DestinationLocationCity,
            DestinationLocationCountry = t.DestinationLocationCountry,
            DestinationLocationTimeZoneId = t.DestinationLocationTimeZoneId,
            PlannedDepartureTimeUtc = t.PlannedDepartureTimeUtc,
            PlannedArrivalTimeUtc = t.PlannedArrivalTimeUtc,
            StartedAtUtc = t.StartedAtUtc,
            EndedAtUtc = t.EndedAtUtc,
            DrivingMinutes = t.DrivingMinutes,
            FinalStatus = t.FinalStatus
        };

        public class ByDriverId : Specification<DriverTrip>
        {
            public ByDriverId(int driverId)
            {
                Query.Where(t => t.DriverId == driverId);
            }
        }

        public class ByRouteId : Specification<DriverTrip>
        {
            public ByRouteId(int routeId)
            {
                Query.Where(t => t.RouteId == routeId);
            }
        }

        public class PagedListByDriverId : Specification<DriverTrip, GetDriverTripResponseModel>
        {
            public PagedListByDriverId(int driverId, GetPagedListRequestModel<DriverTripSortField> pagedListRequest)
            {
                Query.Where(t => t.DriverId == driverId);

                var sortField = pagedListRequest.Sort ?? DriverTripSortField.StartedAt;
                Expression<Func<DriverTrip, object?>> sortKeySelector = sortField switch
                {
                    DriverTripSortField.StartedAt => t => t.StartedAtUtc,
                    DriverTripSortField.DrivingMinutes => t => t.DrivingMinutes,
                    _ => throw new InvalidOperationException(string.Format(ExceptionMessages.SortFieldNotSupportedTemplate, sortField))
                };

                Query.OrderBySortDirection(sortKeySelector, t => t.Id, pagedListRequest.SortDirection ?? SortDirection.Descending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(GetDriverTripResponseModelSelector);
            }
        }
    }
}
