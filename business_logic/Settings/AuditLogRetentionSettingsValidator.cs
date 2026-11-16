using Microsoft.Extensions.Options;

namespace business_logic.Settings
{
    public class AuditLogRetentionSettingsValidator : IValidateOptions<AuditLogRetentionSettings>
    {
        public ValidateOptionsResult Validate(string? name, AuditLogRetentionSettings auditLogRetentionSettings)
        {
            if (auditLogRetentionSettings.RetentionPeriodInDays <= 0)
            {
                return ValidateOptionsResult.Fail(
                    $"'{AuditLogRetentionSettings.SectionName}:{nameof(AuditLogRetentionSettings.RetentionPeriodInDays)}' must be a positive number of days, but was '{auditLogRetentionSettings.RetentionPeriodInDays}'.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}
