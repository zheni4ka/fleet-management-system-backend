using Ardalis.Specification;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Specifications
{
    public class IdentityUserRoleSpecs
    {
        public class UserIdsByRoleIds : Specification<IdentityUserRole<string>, string>
        {
            public UserIdsByRoleIds(IQueryable<string> roleIdsQuery)
            {
                Query.Where(ur => roleIdsQuery.Contains(ur.RoleId));
                Query.Select(ur => ur.UserId);
            }
        }
    }
}
