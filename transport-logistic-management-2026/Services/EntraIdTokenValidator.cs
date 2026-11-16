using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using transport_logistic_management_2026.Constants;

namespace transport_logistic_management_2026.Services
{
    public class EntraIdTokenValidator : IExternalIdentityTokenValidator
    {
        private readonly ExternalIdentitySettings _externalIdentitySettings;
        private readonly ConfigurationManager<OpenIdConnectConfiguration>? _openIdConnectConfigurationManager;
        private readonly ILogger<EntraIdTokenValidator> _logger;

        public EntraIdTokenValidator(IOptions<ExternalIdentitySettings> externalIdentitySettingsOptions, ILogger<EntraIdTokenValidator> logger)
        {
            _externalIdentitySettings = externalIdentitySettingsOptions.Value;
            _logger = logger;
            if (string.IsNullOrWhiteSpace(_externalIdentitySettings.Authority))
            {
                return;
            }

            _openIdConnectConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                _externalIdentitySettings.Authority.TrimEnd('/') + OpenIdConnectPaths.MetadataPath,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true });
        }

        public async Task<ValidatedExternalIdentityModel> ValidateIdTokenAsync(string idToken, string expectedNonce)
        {
            if (_openIdConnectConfigurationManager is null)
            {
                throw new BusinessRuleException(ExceptionMessages.ExternalIdentityNotConfigured);
            }

            var openIdConnectConfiguration = await _openIdConnectConfigurationManager.GetConfigurationAsync(CancellationToken.None);
            var idTokenValidationResult = await new JsonWebTokenHandler().ValidateTokenAsync(idToken, new TokenValidationParameters
            {
                ValidIssuer = openIdConnectConfiguration.Issuer,
                ValidAudience = _externalIdentitySettings.ClientId,
                IssuerSigningKeys = openIdConnectConfiguration.SigningKeys,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            });
            if (!idTokenValidationResult.IsValid)
            {
                _logger.LogWarning(idTokenValidationResult.Exception, "External identity token validation failed.");
                throw new UnauthorizedAccessException(ExceptionMessages.ExternalIdentityTokenInvalid);
            }

            var idTokenIdentity = idTokenValidationResult.ClaimsIdentity;
            if (idTokenIdentity.FindFirst(EntraIdClaimTypes.Nonce)?.Value != expectedNonce)
            {
                _logger.LogWarning("External identity token nonce does not match the login transaction.");
                throw new UnauthorizedAccessException(ExceptionMessages.ExternalIdentityTokenInvalid);
            }

            var objectId = GetRequiredClaimValue(idTokenIdentity, EntraIdClaimTypes.ObjectId);
            var tenantId = GetRequiredClaimValue(idTokenIdentity, EntraIdClaimTypes.TenantId);

            return new ValidatedExternalIdentityModel(
                SubjectId: $"{tenantId}:{objectId}",
                PreferredUsername: idTokenIdentity.FindFirst(EntraIdClaimTypes.PreferredUsername)?.Value,
                Email: idTokenIdentity.FindFirst(EntraIdClaimTypes.Email)?.Value,
                GroupIds: idTokenIdentity.FindAll(EntraIdClaimTypes.Groups).Select(groupClaim => groupClaim.Value).ToList());
        }

        private string GetRequiredClaimValue(ClaimsIdentity idTokenIdentity, string claimType)
        {
            var claimValue = idTokenIdentity.FindFirst(claimType)?.Value;
            if (string.IsNullOrWhiteSpace(claimValue))
            {
                _logger.LogWarning("External identity token has no '{ClaimType}' claim.", claimType);
                throw new UnauthorizedAccessException(ExceptionMessages.ExternalIdentityTokenInvalid);
            }

            return claimValue;
        }
    }
}
