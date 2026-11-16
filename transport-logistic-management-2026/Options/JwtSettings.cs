namespace transport_logistic_management_2026.Options
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";
        public const int SecretMinLength = 32;

        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int AccessTokenExpiryInMinutes { get; set; }
    }
}
