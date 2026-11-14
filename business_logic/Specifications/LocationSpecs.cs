using Ardalis.Specification;
using business_logic.DTOs;
using business_logic.Entities;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class LocationSpecs
    {
        public class PagedList : Specification<Location>
        {
            public PagedList(
                GetLocationsBySearchFilterRequestModel searchFilter,
                GetPagedListRequestModel<LocationSortField> pagedListRequest)
            {
                if (!string.IsNullOrWhiteSpace(searchFilter.Country))
                {
                    var trimmedCountry = searchFilter.Country.Trim();
                    Query.Where(l => l.Country.Contains(trimmedCountry));
                }

                if (!string.IsNullOrWhiteSpace(pagedListRequest.Search))
                {
                    var trimmedSearch = pagedListRequest.Search.Trim();
                    Query.Where(l => l.City.Contains(trimmedSearch) || l.Country.Contains(trimmedSearch));
                }

                var sortField = pagedListRequest.Sort ?? LocationSortField.Id;
                Expression<Func<Location, object?>> sortKeySelector = sortField switch
                {
                    LocationSortField.Id => l => l.Id,
                    LocationSortField.City => l => l.City,
                    LocationSortField.Country => l => l.Country,
                    _ => throw new Exception($"Location sort field '{sortField}' is not supported.")
                };

                Query.OrderBySortDirection(sortKeySelector, l => l.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
            }
        }
    }
}
