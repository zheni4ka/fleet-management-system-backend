using Microsoft.Extensions.Options;

namespace transport_logistic_management_2026.Options
{
    public class JwtSettingsValidator : IValidateOptions<JwtSettings>
    {
        public ValidateOptionsResult Validate(string? name, JwtSettings jwtSettings)
        {
            if (jwtSettings.Secret.Length < JwtSettings.SecretMinLength)
            {
                return ValidateOptionsResult.Fail(
                    $"'{JwtSettings.SectionName}:{nameof(JwtSettings.Secret)}' must be at least {JwtSettings.SecretMinLength} characters long, but has {jwtSettings.Secret.Length}.");
            }

            if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            {
                return ValidateOptionsResult.Fail($"'{JwtSettings.SectionName}:{nameof(JwtSettings.Issuer)}' is not configured.");
            }

            if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
            {
                return ValidateOptionsResult.Fail($"'{JwtSettings.SectionName}:{nameof(JwtSettings.Audience)}' is not configured.");
            }

            if (jwtSettings.AccessTokenExpiryInMinutes <= 0)
            {
                return ValidateOptionsResult.Fail(
                    $"'{JwtSettings.SectionName}:{nameof(JwtSettings.AccessTokenExpiryInMinutes)}' must be a positive number of minutes, but was '{jwtSettings.AccessTokenExpiryInMinutes}'.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
