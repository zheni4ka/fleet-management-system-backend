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
            public ByLicenseNumber(string licenseNumber)
            {
                Query.Where(d => d.LicenseNumber == licenseNumber);
            }
        }

        public class All : Specification<Driver>
        {
            public All()
            {
                Query.OrderBy(d => d.Id);
            }
        }

        public class PagedList : Specification<Driver>
        {
            public PagedList(GetPagedListRequestModel<DriverSortField> pagedListRequest)
            {
                if (!string.IsNullOrWhiteSpace(pagedListRequest.Search))
                {
                    var trimmedSearch = pagedListRequest.Search.Trim();
                    Query.Where(d => d.Name.Contains(trimmedSearch)
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
            }
        }
    }
}
