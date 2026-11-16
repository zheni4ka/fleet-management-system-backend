using Ardalis.Specification;
using business_logic.DTOs;
using business_logic.Entities;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class DriverSpecs
    {
        public class ById : Specification<Driver>
        {
            public ById(int id)
            {
                Query.Where(d => d.Id == id);
            }
        }

        public class ByName : Specification<Driver>
        {
            public ByName(string name)
            {
                Query.Where(d => d.Name == name);
            }
        }

        public class ByLicenseNumber : Specification<Driver>
        {
            public ByLicenseNumber(string licenseNumber, int? excludedDriverId = null)
            {
                Query.Where(d => d.LicenseNumber == licenseNumber);

                if (excludedDriverId.HasValue)
                {
                    var excludedDriverIdValue = excludedDriverId.Value;
                    Query.Where(d => d.Id != excludedDriverIdValue);
                }
            }
        }

        public class All : Specification<Driver>
        {
            public All()
            {
                Query.OrderBy(d => d.Id);
            }
        }

        public class PagedList : Specification<Driver, GetDriverResponseModel>
        {
            public PagedList(GetPagedListRequestModel<DriverSortField> pagedListRequest)
            {
                if (!string.IsNullOrWhiteSpace(pagedListRequest.Search))
                {
                    var trimmedSearch = pagedListRequest.Search.Trim();
                    var searchedId = SearchQueryIdParser.ParseSearchedId(trimmedSearch);
                    Query.Where(d => (searchedId != null && d.Id == searchedId)
                        || d.Name.Contains(trimmedSearch)
                        || d.Surname.Contains(trimmedSearch)
                        || d.Patronymic.Contains(trimmedSearch)
                        || d.LicenseNumber.Contains(trimmedSearch));
                }

                var sortField = pagedListRequest.Sort ?? DriverSortField.Id;
                Expression<Func<Driver, object?>> sortKeySelector = sortField switch
                {
                    DriverSortField.Id => d => d.Id,
                    DriverSortField.Name => d => d.Name,
                    DriverSortField.Surname => d => d.Surname,
                    DriverSortField.Patronymic => d => d.Patronymic,
                    DriverSortField.LicenseNumber => d => d.LicenseNumber,
                    _ => throw new Exception($"Driver sort field '{sortField}' is not supported.")
                };

                Query.OrderBySortDirection(sortKeySelector, d => d.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(d => new GetDriverResponseModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Surname = d.Surname,
                    Patronymic = d.Patronymic,
                    LicenseNumber = d.LicenseNumber
                });
            }
        }

        public class RoutesPeriodReportSummaries : Specification<Driver, GetRoutesPeriodReportDriverSummaryResponseModel>
        {
            public RoutesPeriodReportSummaries(DateTime periodStartDateTime, DateTime periodEndExclusiveDateTime)
            {
                Query.Where(d => d.Routes.Any(r => r.DepartureTime >= periodStartDateTime && r.DepartureTime < periodEndExclusiveDateTime))
                    .OrderByDescending(d => d.Routes.Count(r => r.DepartureTime >= periodStartDateTime && r.DepartureTime < periodEndExclusiveDateTime))
                    .ThenBy(d => d.Id);
                Query.Select(d => new GetRoutesPeriodReportDriverSummaryResponseModel
                {
                    DriverId = d.Id,
                    DriverName = d.Name,
                    DriverSurname = d.Surname,
                    RoutesCount = d.Routes.Count(r => r.DepartureTime >= periodStartDateTime && r.DepartureTime < periodEndExclusiveDateTime),
                    CompletedRoutesCount = d.Routes.Count(r => r.DepartureTime >= periodStartDateTime
                        && r.DepartureTime < periodEndExclusiveDateTime
                        && r.Status == RouteStatus.Completed)
                });
            }
        }

        public class LookupOptions : Specification<Driver, GetDriverLookupOptionResponseModel>
        {
            public LookupOptions()
            {
                Query.OrderBy(d => d.Surname)
                    .ThenBy(d => d.Name)
                    .ThenBy(d => d.Id);
                Query.Select(d => new GetDriverLookupOptionResponseModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Surname = d.Surname,
                    LicenseNumber = d.LicenseNumber
                });
            }
        }
    }
}
