namespace transport_logistic_management_2026.Options
{
    public class ObservabilitySettings
    {
        public const string SectionName = "ObservabilitySettings";

        public string ServiceName { get; set; } = string.Empty;
        public string OtlpEndpoint { get; set; } = string.Empty;
    }
}
