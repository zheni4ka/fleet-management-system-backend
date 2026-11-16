using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Services
{
    public class SessionResponseFactory : ISessionResponseFactory
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;

        public SessionResponseFactory(UserManager<IdentityUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<TSessionResponseModel> CreateSessionResponseModelAsync<TSessionResponseModel>(
            IdentityUser user,
            IssuedRefreshTokenModel issuedRefreshToken)
            where TSessionResponseModel : SessionResponseModelBase, new()
        {
            var roles = await _userManager.GetRolesAsync(user);
            var issuedAccessToken = _tokenService.CreateAccessToken(user, roles);

            return new TSessionResponseModel
            {
                AccessToken = issuedAccessToken.AccessToken,
                AccessTokenExpiresAtUtc = issuedAccessToken.ExpiresAtUtc,
                RefreshToken = issuedRefreshToken.RefreshToken,
                RefreshTokenExpiresAtUtc = issuedRefreshToken.ExpiresAtUtc,
                Username = user.UserName!,
                Email = user.Email,
                Roles = roles.ToList()
            };
        }
    }
}
