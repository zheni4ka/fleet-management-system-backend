using Microsoft.Extensions.Options;
using System.Net;

namespace transport_logistic_management_2026.Options
{
    public class ForwardedHeadersSettingsValidator : IValidateOptions<ForwardedHeadersSettings>
    {
        public ValidateOptionsResult Validate(string? name, ForwardedHeadersSettings forwardedHeadersSettings)
        {
            foreach (var knownProxyAddress in forwardedHeadersSettings.KnownProxyAddresses)
            {
                if (!IPAddress.TryParse(knownProxyAddress, out _))
                {
                    return ValidateOptionsResult.Fail(
                        $"'{ForwardedHeadersSettings.SectionName}:{nameof(ForwardedHeadersSettings.KnownProxyAddresses)}' contains '{knownProxyAddress}', which is not an IP address.");
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}
