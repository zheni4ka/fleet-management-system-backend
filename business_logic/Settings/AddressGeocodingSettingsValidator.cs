using Microsoft.Extensions.Options;

namespace business_logic.Settings
{
    public class AddressGeocodingSettingsValidator : IValidateOptions<AddressGeocodingSettings>
    {
        public ValidateOptionsResult Validate(string? name, AddressGeocodingSettings addressGeocodingSettings)
        {
            var isBaseUrlAbsoluteHttps = Uri.TryCreate(addressGeocodingSettings.BaseUrl, UriKind.Absolute, out var baseUri)
                && baseUri.Scheme == Uri.UriSchemeHttps
                && addressGeocodingSettings.BaseUrl.EndsWith('/');
            if (!isBaseUrlAbsoluteHttps)
            {
                return ValidateOptionsResult.Fail(
                    $"'{AddressGeocodingSettings.SectionName}:{nameof(AddressGeocodingSettings.BaseUrl)}' must be an absolute https URL ending with '/', but was '{addressGeocodingSettings.BaseUrl}'.");
            }

            if (addressGeocodingSettings.RequestTimeoutInSeconds <= 0)
            {
                return ValidateOptionsResult.Fail(
                    $"'{AddressGeocodingSettings.SectionName}:{nameof(AddressGeocodingSettings.RequestTimeoutInSeconds)}' must be a positive number of seconds, but was '{addressGeocodingSettings.RequestTimeoutInSeconds}'.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
