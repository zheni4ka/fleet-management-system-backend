using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using transport_logistic_management_2026.Options;

namespace transport_logistic_management_2026.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public IssuedAccessTokenModel CreateAccessToken(IdentityUser user, IReadOnlyCollection<string> roles, IReadOnlyCollection<int> depotIds)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange(depotIds.Select(depotId => new Claim(DepotClaimTypes.DepotId, depotId.ToString(CultureInfo.InvariantCulture))));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var accessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryInMinutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = accessTokenExpiresAtUtc,
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            return new IssuedAccessTokenModel(
                AccessToken: handler.WriteToken(handler.CreateToken(descriptor)),
                ExpiresAtUtc: new DateTimeOffset(accessTokenExpiresAtUtc));
        }
    }
}
