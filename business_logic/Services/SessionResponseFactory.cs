using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic.Specifications;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Services
{
    public class SessionResponseFactory : ISessionResponseFactory
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IRepository<UserDepot> _userDepotRepository;

        public SessionResponseFactory(UserManager<IdentityUser> userManager, ITokenService tokenService, IRepository<UserDepot> userDepotRepository)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _userDepotRepository = userDepotRepository;
        }

        public async Task<TSessionResponseModel> CreateSessionResponseModelAsync<TSessionResponseModel>(
            IdentityUser user,
            IssuedRefreshTokenModel issuedRefreshToken)
            where TSessionResponseModel : SessionResponseModelBase, new()
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var depotIds = await _userDepotRepository.GetProjectedListBySpecAsync(new UserDepotSpecs.DepotIdsByUserId(user.Id));
            var preferredLanguageClaim = (await _userManager.GetClaimsAsync(user))
                .FirstOrDefault(userClaim => userClaim.Type == AccountClaimTypes.PreferredLanguage);
            var issuedAccessToken = _tokenService.CreateAccessToken(user, roles, depotIds);

            return new TSessionResponseModel
            {
                AccessToken = issuedAccessToken.AccessToken,
                AccessTokenExpiresAtUtc = issuedAccessToken.ExpiresAtUtc,
                RefreshToken = issuedRefreshToken.RefreshToken,
                RefreshTokenExpiresAtUtc = issuedRefreshToken.ExpiresAtUtc,
                Username = user.UserName!,
                Email = user.Email,
                Roles = roles,
                PreferredLanguage = preferredLanguageClaim is null ? SupportedLanguages.Default : preferredLanguageClaim.Value
            };
        }
    }
}
