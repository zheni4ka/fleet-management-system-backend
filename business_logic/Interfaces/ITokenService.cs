using Microsoft.AspNetCore.Identity;

namespace business_logic.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(IdentityUser user, IEnumerable<string> roles);
    }
}
