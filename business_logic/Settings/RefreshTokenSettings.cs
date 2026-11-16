namespace business_logic.Settings
{
    public class RefreshTokenSettings
    {
        public const string SectionName = "RefreshTokenSettings";

        public int ExpiryInDays { get; set; }
        public int RotatedTokenReuseGracePeriodInSeconds { get; set; }
    }
}
