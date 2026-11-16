using Microsoft.Extensions.Options;

namespace transport_logistic_management_2026.Options
{
    public class RateLimitingSettingsValidator : IValidateOptions<RateLimitingSettings>
    {
        public ValidateOptionsResult Validate(string? name, RateLimitingSettings rateLimitingSettings)
        {
            var positiveRateLimitingValues = new Dictionary<string, int>
            {
                [nameof(RateLimitingSettings.CredentialEndpointsPermitLimitPerIpAddress)] = rateLimitingSettings.CredentialEndpointsPermitLimitPerIpAddress,
                [nameof(RateLimitingSettings.CredentialEndpointsWindowInSeconds)] = rateLimitingSettings.CredentialEndpointsWindowInSeconds,
                [nameof(RateLimitingSettings.GeneralPermitLimitPerUser)] = rateLimitingSettings.GeneralPermitLimitPerUser,
                [nameof(RateLimitingSettings.GeneralWindowInSeconds)] = rateLimitingSettings.GeneralWindowInSeconds
            };

            foreach (var (rateLimitingSettingName, rateLimitingSettingValue) in positiveRateLimitingValues)
            {
                if (rateLimitingSettingValue <= 0)
                {
                    return ValidateOptionsResult.Fail(
                        $"'{RateLimitingSettings.SectionName}:{rateLimitingSettingName}' must be a positive number, but was '{rateLimitingSettingValue}'.");
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}
