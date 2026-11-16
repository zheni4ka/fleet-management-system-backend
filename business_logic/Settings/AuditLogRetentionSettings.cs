namespace business_logic.Settings
{
    public class AuditLogRetentionSettings
    {
        public const string SectionName = "AuditLogRetentionSettings";

        public int RetentionPeriodInDays { get; set; }
    }
}
