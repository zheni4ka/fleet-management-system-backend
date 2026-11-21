namespace business_logic.Settings
{
    public class AddressGeocodingSettings
    {
        public const string SectionName = "AddressGeocodingSettings";

        public string BaseUrl { get; set; } = string.Empty;
        public int RequestTimeoutInSeconds { get; set; }
    }
}
