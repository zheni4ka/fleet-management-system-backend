using Microsoft.Extensions.Options;

namespace business_logic.Settings
{
    public class RefreshTokenSettingsValidator : IValidateOptions<RefreshTokenSettings>
    {
        public ValidateOptionsResult Validate(string? name, RefreshTokenSettings refreshTokenSettings)
        {
            if (refreshTokenSettings.ExpiryInDays <= 0)
            {
                return ValidateOptionsResult.Fail(
                    $"'{RefreshTokenSettings.SectionName}:{nameof(RefreshTokenSettings.ExpiryInDays)}' must be a positive number of days, but was '{refreshTokenSettings.ExpiryInDays}'.");
            }

            if (refreshTokenSettings.RotatedTokenReuseGracePeriodInSeconds < 0)
            {
                return ValidateOptionsResult.Fail(
                    $"'{RefreshTokenSettings.SectionName}:{nameof(RefreshTokenSettings.RotatedTokenReuseGracePeriodInSeconds)}' must be zero or a positive number of seconds, but was '{refreshTokenSettings.RotatedTokenReuseGracePeriodInSeconds}'.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
