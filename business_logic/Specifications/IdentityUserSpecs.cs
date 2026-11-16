using Ardalis.Specification;
using business_logic.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class IdentityUserSpecs
    {
        public class DispatchersPagedList : Specification<IdentityUser, UserDTO>
        {
            public DispatchersPagedList(
                IQueryable<string> dispatcherUserIdsQuery,
                GetPagedListRequestModel<DispatcherSortField> pagedListRequest)
            {
                Query.Where(u => dispatcherUserIdsQuery.Contains(u.Id));

                if (!string.IsNullOrWhiteSpace(pagedListRequest.Search))
                {
                    var trimmedSearch = pagedListRequest.Search.Trim();
                    Query.Where(u => u.Id.Contains(trimmedSearch)
                        || (u.UserName != null && u.UserName.Contains(trimmedSearch))
                        || (u.Email != null && u.Email.Contains(trimmedSearch)));
                }

                var sortField = pagedListRequest.Sort ?? DispatcherSortField.Username;
                Expression<Func<IdentityUser, object?>> sortKeySelector = sortField switch
                {
                    DispatcherSortField.Id => u => u.Id,
                    DispatcherSortField.Username => u => u.UserName,
                    DispatcherSortField.Email => u => u.Email,
                    _ => throw new Exception($"Dispatcher sort field '{sortField}' is not supported.")
                };

                Query.OrderBySortDirection(sortKeySelector, u => u.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(u => new UserDTO
                {
                    Id = u.Id,
                    Username = u.UserName!,
                    Email = u.Email
                });
            }
        }
    }
}
