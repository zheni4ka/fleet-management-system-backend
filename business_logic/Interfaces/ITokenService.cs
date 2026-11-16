using business_logic.DTOs;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Interfaces
{
    public interface ITokenService
    {
        IssuedAccessTokenModel CreateAccessToken(IdentityUser user, IEnumerable<string> roles);
    }
}
