using Ardalis.Specification;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class DriverSpecs
    {
        private static readonly Expression<Func<Driver, GetDriverResponseModel>> GetDriverResponseModelSelector = d => new GetDriverResponseModel
        {
            Id = d.Id,
            Name = d.Name,
            Surname = d.Surname,
            Patronymic = d.Patronymic,
            LicenseNumber = d.LicenseNumber,
            DepotId = d.DepotId,
            DepotName = d.Depot!.Name,
            IsArchived = d.IsArchived,
            RowVersion = d.RowVersion
        };

        public class ResponseModelById : Specification<Driver, GetDriverResponseModel>
        {
            public ResponseModelById(int id)
            {
                Query.Where(d => d.Id == id);
                Query.Select(GetDriverResponseModelSelector);
            }
        }

        public class AssignmentStateById : Specification<Driver, AssignmentStateModel>
        {
            public AssignmentStateById(int id)
            {
                Query.Where(d => d.Id == id);
                Query.Select(d => new AssignmentStateModel(d.DepotId, d.IsArchived));
            }
        }

        public class ByDepotId : Specification<Driver>
        {
            public ByDepotId(int depotId)
            {
                Query.Where(d => d.DepotId == depotId);
            }
        }

        public class Unarchived : Specification<Driver>
        {
            public Unarchived()
            {
                Query.Where(d => !d.IsArchived);
            }
        }

        public class ById : Specification<Driver>
        {
            public ById(int id)
            {
                Query.Where(d => d.Id == id);
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
            public PagedList(
                GetDriversBySearchFilterRequestModel searchFilter,
                GetPagedListRequestModel<DriverSortField> pagedListRequest)
            {
                var isArchivedSearched = searchFilter.IsArchived;
                Query.Where(d => d.IsArchived == isArchivedSearched);

                if (searchFilter.DepotId.HasValue)
                {
                    var searchedDepotId = searchFilter.DepotId.Value;
                    Query.Where(d => d.DepotId == searchedDepotId);
                }

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
                    _ => throw new Exception(string.Format(ExceptionMessages.SortFieldNotSupportedTemplate, sortField))
                };

                Query.OrderBySortDirection(sortKeySelector, d => d.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(GetDriverResponseModelSelector);
            }
        }

        public class RoutesPeriodReportSummaries : Specification<Driver, GetRoutesPeriodReportDriverSummaryResponseModel>
        {
            public RoutesPeriodReportSummaries(DateTime periodStartUtc, DateTime periodEndExclusiveUtc)
            {
                var routeOverlapsPeriodCriteria = RouteSpecs.CreateOverlappingPeriodCriteria(periodStartUtc, periodEndExclusiveUtc);
                Query.Where(d => d.Routes.AsQueryable().Any(routeOverlapsPeriodCriteria))
                    .OrderByDescending(d => d.Routes.AsQueryable().Count(routeOverlapsPeriodCriteria))
                    .ThenBy(d => d.Id);
                Query.Select(d => new GetRoutesPeriodReportDriverSummaryResponseModel
                {
                    DriverId = d.Id,
                    DriverName = d.Name,
                    DriverSurname = d.Surname,
                    RoutesCount = d.Routes.AsQueryable().Count(routeOverlapsPeriodCriteria),
                    CompletedRoutesCount = d.Routes.AsQueryable().Where(routeOverlapsPeriodCriteria).Count(r => r.Status == RouteStatus.Completed)
                });
            }
        }

        public class LookupOptions : Specification<Driver, GetDriverLookupOptionResponseModel>
        {
            public LookupOptions()
            {
                Query.Where(d => !d.IsArchived);
                Query.OrderBy(d => d.Surname)
                    .ThenBy(d => d.Name)
                    .ThenBy(d => d.Id);
                Query.Select(d => new GetDriverLookupOptionResponseModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Surname = d.Surname,
                    LicenseNumber = d.LicenseNumber,
                    DepotId = d.DepotId
                });
            }
        }
    }
}
