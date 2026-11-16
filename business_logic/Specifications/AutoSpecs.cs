using Ardalis.Specification;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class AutoSpecs
    {
        public class ById : Specification<Auto>
        {
            public ById(int id)
            {
                Query.Where(a => a.Id == id);
            }
        }

        public class ByNumber : Specification<Auto>
        {
            public ByNumber(string number, int? excludedAutoId = null)
            {
                Query.Where(a => a.Number == number);

                if (excludedAutoId.HasValue)
                {
                    var excludedAutoIdValue = excludedAutoId.Value;
                    Query.Where(a => a.Id != excludedAutoIdValue);
                }
            }
        }

        public class All : Specification<Auto>
        {
            public All()
            {
                Query.OrderBy(a => a.Mark);
            }
        }

        public class PagedList : Specification<Auto, GetAutoResponseModel>
        {
            public PagedList(
                GetAutosBySearchFilterRequestModel searchFilter,
                GetPagedListRequestModel<AutoSortField> pagedListRequest)
            {
                if (searchFilter.Status.HasValue)
                {
                    var searchedAutoStatus = searchFilter.Status.Value;
                    Query.Where(a => a.Status == searchedAutoStatus);
                }

                if (!string.IsNullOrWhiteSpace(searchFilter.Color))
                {
                    var trimmedColor = searchFilter.Color.Trim();
                    Query.Where(a => a.Color == trimmedColor);
                }

                if (!string.IsNullOrWhiteSpace(pagedListRequest.Search))
                {
                    var trimmedSearch = pagedListRequest.Search.Trim();
                    var searchedId = SearchQueryIdParser.ParseSearchedId(trimmedSearch);
                    Query.Where(a => (searchedId != null && a.Id == searchedId)
                        || a.Mark.Contains(trimmedSearch)
                        || a.Model.Contains(trimmedSearch)
                        || a.Color.Contains(trimmedSearch)
                        || a.Number.Contains(trimmedSearch));
                }

                var sortField = pagedListRequest.Sort ?? AutoSortField.Mark;
                Expression<Func<Auto, object?>> sortKeySelector = sortField switch
                {
                    AutoSortField.Id => a => a.Id,
                    AutoSortField.Mark => a => a.Mark,
                    AutoSortField.Model => a => a.Model,
                    AutoSortField.Color => a => a.Color,
                    AutoSortField.Number => a => a.Number,
                    AutoSortField.Capacity => a => a.Capacity,
                    AutoSortField.Status => a => a.Status,
                    _ => throw new Exception(string.Format(ExceptionMessages.SortFieldNotSupportedTemplate, sortField))
                };

                Query.OrderBySortDirection(sortKeySelector, a => a.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(a => new GetAutoResponseModel
                {
                    Id = a.Id,
                    Mark = a.Mark,
                    Model = a.Model,
                    Color = a.Color,
                    Number = a.Number,
                    Capacity = a.Capacity,
                    Status = a.Status
                });
            }
        }

        public class WithRoutesDepartingInPeriod : Specification<Auto>
        {
            public WithRoutesDepartingInPeriod(DateTime periodStartDateTime, DateTime periodEndExclusiveDateTime)
            {
                Query.Where(a => a.Routes.Any(r => r.DepartureTime >= periodStartDateTime && r.DepartureTime < periodEndExclusiveDateTime));
            }
        }

        public class AwaitingUnderMaintenanceStatusByDueMaintenance : Specification<Auto>
        {
            public AwaitingUnderMaintenanceStatusByDueMaintenance(DateTime dueMaintenanceEndExclusiveDateTime)
            {
                Query.Where(a => a.Status != AutoStatus.UnderMaintenance
                    && a.Services.Any(am => !am.IsCompleted && am.ServiceDate < dueMaintenanceEndExclusiveDateTime)
                    && !a.Routes.Any(r => r.Status == RouteStatus.InProgress));
            }
        }

        public class LookupOptions : Specification<Auto, GetAutoLookupOptionResponseModel>
        {
            public LookupOptions()
            {
                Query.OrderBy(a => a.Mark)
                    .ThenBy(a => a.Model)
                    .ThenBy(a => a.Number);
                Query.Select(a => new GetAutoLookupOptionResponseModel
                {
                    Id = a.Id,
                    Mark = a.Mark,
                    Model = a.Model,
                    Number = a.Number,
                    Status = a.Status
                });
            }
        }
    }
}
