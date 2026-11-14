using Ardalis.Specification;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Specifications
{
    public class IdentityUserRoleSpecs
    {
        public class UserIdsByRoleId : Specification<IdentityUserRole<string>, string>
        {
            public UserIdsByRoleId(string roleId)
            {
                Query.Where(ur => ur.RoleId == roleId);
                Query.Select(ur => ur.UserId);
            }
        }
    }
}
