using business_logic.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace business_logic_tests.Infrastructure
{
    public sealed class TestTokenService : ITokenService
    {
        public string CreateToken(IdentityUser user, IEnumerable<string> roles)
        {
            return $"test-token-{user.Id}";
        }
    }
}
