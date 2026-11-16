using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace business_logic_tests.Infrastructure
{
    public sealed class TestTokenService : ITokenService
    {
        public IssuedAccessTokenModel CreateAccessToken(IdentityUser user, IEnumerable<string> roles)
        {
            return new IssuedAccessTokenModel(AccessToken: $"test-token-{user.Id}", ExpiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(15));
        }
    }
}
