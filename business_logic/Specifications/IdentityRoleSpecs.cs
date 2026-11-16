using Ardalis.Specification;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Specifications
{
    public class IdentityRoleSpecs
    {
        public class IdsByName : Specification<IdentityRole, string>
        {
            public IdsByName(string roleName)
            {
                Query.Where(r => r.Name == roleName);
                Query.Select(r => r.Id);
            }
        }
    }
}
