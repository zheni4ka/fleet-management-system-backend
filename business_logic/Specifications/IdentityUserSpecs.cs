using Ardalis.Specification;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace business_logic.Specifications
{
    public class IdentityUserSpecs
    {
        private static Expression<Func<IdentityUser, GetDispatcherResponseModel>> CreateGetDispatcherResponseModelSelector(IQueryable<UserDepot> userDepotsQuery)
        {
            var deactivatedUserLockoutEnd = AccountConstraints.DeactivatedUserLockoutEnd;
            return u => new GetDispatcherResponseModel
            {
                Id = u.Id,
                Username = u.UserName!,
                Email = u.Email,
                IsDeactivated = u.LockoutEnd == deactivatedUserLockoutEnd,
                DepotIds = userDepotsQuery
                    .Where(userDepot => userDepot.UserId == u.Id)
                    .OrderBy(userDepot => userDepot.DepotId)
                    .Select(userDepot => userDepot.DepotId)
                    .ToList()
            };
        }

        public class DispatcherById : Specification<IdentityUser, GetDispatcherResponseModel>
        {
            public DispatcherById(string id, IQueryable<string> dispatcherUserIdsQuery, IQueryable<UserDepot> userDepotsQuery)
            {
                Query.Where(u => u.Id == id && dispatcherUserIdsQuery.Contains(u.Id));
                Query.Select(CreateGetDispatcherResponseModelSelector(userDepotsQuery));
            }
        }

        public class DispatchersPagedList : Specification<IdentityUser, GetDispatcherResponseModel>
        {
            public DispatchersPagedList(
                IQueryable<string> dispatcherUserIdsQuery,
                IQueryable<UserDepot> userDepotsQuery,
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
                    _ => throw new Exception(string.Format(ExceptionMessages.SortFieldNotSupportedTemplate, sortField))
                };

                Query.OrderBySortDirection(sortKeySelector, u => u.Id, pagedListRequest.SortDirection ?? SortDirection.Ascending);
                Query.ApplyPage(pagedListRequest.Page, pagedListRequest.PageSize);
                Query.Select(CreateGetDispatcherResponseModelSelector(userDepotsQuery));
            }
        }
    }
}
