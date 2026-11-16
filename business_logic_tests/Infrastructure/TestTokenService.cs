using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace business_logic_tests.Infrastructure
{
    public sealed class TestTokenService : ITokenService
    {
        public IssuedAccessTokenModel CreateAccessToken(IdentityUser user, IReadOnlyCollection<string> roles, IReadOnlyCollection<int> depotIds)
        {
            return new IssuedAccessTokenModel(AccessToken: $"test-token-{user.Id}", ExpiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(15));
        }
    }
}
