using Ardalis.Specification;
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

        public class ByName : Specification<Auto>
        {
            public ByName(string name)
            {
                Query.Where(a => a.Mark == name);
            }
        }

        public class ByNumber : Specification<Auto>
        {
            public ByNumber(string number)
            {
                Query.Where(a => a.Number == number);
            }
        }

        public class All : Specification<Auto>
        {
            public All()
            {
                Query.OrderBy(a => a.Mark);
            }
        }

        public class CapacityMoreThan : Specification<Auto>
        {
            public CapacityMoreThan(double value)
            {
                Query.Where(a => a.Capacity >= value);
            }
        }

        public class CapacityLessThan : Specification<Auto>
        {
            public CapacityLessThan(double value)
            {
                Query.Where(a => a.Capacity <= value);
            }
        }

        public class ByColor : Specification<Auto>
        {
            public ByColor(string color)
            {
                Query.Where(a => a.Color == color);
            }
        }

        public class PagedList : Specification<Auto>
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
                    _ => throw new Exception($"Auto sort field '{sortField}' is not supported.")
                };

                Query.OrderBySortDirection(sortKeySelector, a => a.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
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
    }
}
