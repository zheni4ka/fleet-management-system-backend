using Ardalis.Specification;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class LocationSpecs
    {
        public class ById : Specification<Location>
        {
            public ById(int id)
            {
                Query.Where(l => l.Id == id);
            }
        }

        public class LookupOptions : Specification<Location, GetLocationLookupOptionResponseModel>
        {
            public LookupOptions()
            {
                Query.OrderBy(l => l.Country)
                    .ThenBy(l => l.City)
                    .ThenBy(l => l.Id);
                Query.Select(l => new GetLocationLookupOptionResponseModel
                {
                    Id = l.Id,
                    City = l.City,
                    Country = l.Country
                });
            }
        }

        public class ExistingIdsByIds : Specification<Location, int>
        {
            public ExistingIdsByIds(IReadOnlyCollection<int> locationIds)
            {
                Query.Where(l => locationIds.Contains(l.Id));
                Query.Select(l => l.Id);
            }
        }

        public class PagedList : Specification<Location, GetLocationResponseModel>
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
                    var searchedId = SearchQueryIdParser.ParseSearchedId(trimmedSearch);
                    Query.Where(l => (searchedId != null && l.Id == searchedId)
                        || l.City.Contains(trimmedSearch)
                        || l.Country.Contains(trimmedSearch));
                }

                var sortField = pagedListRequest.Sort ?? LocationSortField.Id;
                Expression<Func<Location, object?>> sortKeySelector = sortField switch
                {
                    LocationSortField.Id => l => l.Id,
                    LocationSortField.City => l => l.City,
                    LocationSortField.Country => l => l.Country,
                    _ => throw new Exception(string.Format(ExceptionMessages.SortFieldNotSupportedTemplate, sortField))
                };

                Query.OrderBySortDirection(sortKeySelector, l => l.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(l => new GetLocationResponseModel
                {
                    Id = l.Id,
                    City = l.City,
                    Country = l.Country
                });
            }
        }
    }
}
