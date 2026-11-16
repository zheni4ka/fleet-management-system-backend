using Microsoft.Extensions.Options;

namespace transport_logistic_management_2026.Options
{
    public class ObservabilitySettingsValidator : IValidateOptions<ObservabilitySettings>
    {
        public ValidateOptionsResult Validate(string? name, ObservabilitySettings observabilitySettings)
        {
            if (string.IsNullOrWhiteSpace(observabilitySettings.ServiceName))
            {
                return ValidateOptionsResult.Fail($"'{ObservabilitySettings.SectionName}:{nameof(ObservabilitySettings.ServiceName)}' is not configured.");
            }

            var isOtlpExporterDisabled = string.IsNullOrWhiteSpace(observabilitySettings.OtlpEndpoint);
            if (!isOtlpExporterDisabled && !Uri.TryCreate(observabilitySettings.OtlpEndpoint, UriKind.Absolute, out _))
            {
                return ValidateOptionsResult.Fail(
                    $"'{ObservabilitySettings.SectionName}:{nameof(ObservabilitySettings.OtlpEndpoint)}' must be an absolute URI or empty, but was '{observabilitySettings.OtlpEndpoint}'.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
