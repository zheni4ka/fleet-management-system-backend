namespace transport_logistic_management_2026.Options
{
    public class RateLimitingSettings
    {
        public const string SectionName = "RateLimitingSettings";

        public int CredentialEndpointsPermitLimitPerIpAddress { get; set; }
        public int CredentialEndpointsWindowInSeconds { get; set; }
        public int GeneralPermitLimitPerUser { get; set; }
        public int GeneralWindowInSeconds { get; set; }
    }
}
