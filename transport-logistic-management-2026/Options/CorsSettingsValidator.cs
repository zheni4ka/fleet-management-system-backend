using Microsoft.Extensions.Options;

namespace transport_logistic_management_2026.Options
{
    public class CorsSettingsValidator : IValidateOptions<CorsSettings>
    {
        public ValidateOptionsResult Validate(string? name, CorsSettings corsSettings)
        {
            if (corsSettings.AllowedOrigins.Length == 0)
            {
                return ValidateOptionsResult.Fail($"'{CorsSettings.SectionName}:{nameof(CorsSettings.AllowedOrigins)}' must contain at least one origin.");
            }

            foreach (var allowedOrigin in corsSettings.AllowedOrigins)
            {
                var isAbsoluteHttpOrigin = Uri.TryCreate(allowedOrigin, UriKind.Absolute, out var allowedOriginUri)
                    && (allowedOriginUri.Scheme == Uri.UriSchemeHttp || allowedOriginUri.Scheme == Uri.UriSchemeHttps)
                    && allowedOriginUri.AbsolutePath == "/";
                if (!isAbsoluteHttpOrigin)
                {
                    return ValidateOptionsResult.Fail(
                        $"'{CorsSettings.SectionName}:{nameof(CorsSettings.AllowedOrigins)}' contains '{allowedOrigin}', which is not an absolute http(s) origin without a path.");
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}
