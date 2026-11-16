namespace transport_logistic_management_2026.Constants
{
    public static class HealthCheckEndpoints
    {
        public const string LivenessPath = "/health/live";
        public const string ReadinessPath = "/health/ready";
        public const string ReadinessTag = "ready";
        public const string DatabaseCheckName = "database";
    }
}
