using business_logic.Constants;
using Microsoft.Extensions.Options;

namespace business_logic.Settings
{
    public class ExternalIdentitySettingsValidator : IValidateOptions<ExternalIdentitySettings>
    {
        public ValidateOptionsResult Validate(string? name, ExternalIdentitySettings externalIdentitySettings)
        {
            var isExternalIdentityNotConfigured = string.IsNullOrWhiteSpace(externalIdentitySettings.Authority)
                && string.IsNullOrWhiteSpace(externalIdentitySettings.ClientId)
                && externalIdentitySettings.GroupRoleMappings.Count == 0;
            if (isExternalIdentityNotConfigured)
            {
                return ValidateOptionsResult.Success;
            }

            var isAuthorityAbsoluteHttpsUri = Uri.TryCreate(externalIdentitySettings.Authority, UriKind.Absolute, out var authorityUri)
                && authorityUri.Scheme == Uri.UriSchemeHttps;
            if (!isAuthorityAbsoluteHttpsUri)
            {
                return ValidateOptionsResult.Fail(
                    $"'{ExternalIdentitySettings.SectionName}:{nameof(ExternalIdentitySettings.Authority)}' must be an absolute https URI, but was '{externalIdentitySettings.Authority}'.");
            }

            if (string.IsNullOrWhiteSpace(externalIdentitySettings.ClientId))
            {
                return ValidateOptionsResult.Fail(
                    $"'{ExternalIdentitySettings.SectionName}:{nameof(ExternalIdentitySettings.ClientId)}' is not configured although '{nameof(ExternalIdentitySettings.Authority)}' is.");
            }

            if (externalIdentitySettings.GroupRoleMappings.Count == 0)
            {
                return ValidateOptionsResult.Fail(
                    $"'{ExternalIdentitySettings.SectionName}:{nameof(ExternalIdentitySettings.GroupRoleMappings)}' must map at least one group id to a role.");
            }

            foreach (var (groupId, roleName) in externalIdentitySettings.GroupRoleMappings)
            {
                if (!UserRoles.All.Contains(roleName))
                {
                    return ValidateOptionsResult.Fail(
                        $"'{ExternalIdentitySettings.SectionName}:{nameof(ExternalIdentitySettings.GroupRoleMappings)}:{groupId}' maps to '{roleName}', which is not one of: {string.Join(", ", UserRoles.All)}.");
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}
