using business_logic.Constants;
using Microsoft.Extensions.Options;

namespace business_logic.Settings
{
    public class CityGeocodingSettingsValidator : IValidateOptions<CityGeocodingSettings>
    {
        public ValidateOptionsResult Validate(string? name, CityGeocodingSettings cityGeocodingSettings)
        {
            var isBaseUrlAbsoluteHttps = Uri.TryCreate(cityGeocodingSettings.BaseUrl, UriKind.Absolute, out var baseUri)
                && baseUri.Scheme == Uri.UriSchemeHttps
                && cityGeocodingSettings.BaseUrl.EndsWith('/');
            if (!isBaseUrlAbsoluteHttps)
            {
                return ValidateOptionsResult.Fail(
                    $"'{CityGeocodingSettings.SectionName}:{nameof(CityGeocodingSettings.BaseUrl)}' must be an absolute https URL ending with '/', but was '{cityGeocodingSettings.BaseUrl}'.");
            }

            if (!SupportedLanguages.All.Contains(cityGeocodingSettings.StoredNamesLanguageCode))
            {
                return ValidateOptionsResult.Fail(
                    $"'{CityGeocodingSettings.SectionName}:{nameof(CityGeocodingSettings.StoredNamesLanguageCode)}' must be one of '{string.Join(", ", SupportedLanguages.All)}', but was '{cityGeocodingSettings.StoredNamesLanguageCode}'.");
            }

            if (cityGeocodingSettings.RequestTimeoutInSeconds <= 0)
            {
                return ValidateOptionsResult.Fail(
                    $"'{CityGeocodingSettings.SectionName}:{nameof(CityGeocodingSettings.RequestTimeoutInSeconds)}' must be a positive number of seconds, but was '{cityGeocodingSettings.RequestTimeoutInSeconds}'.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
