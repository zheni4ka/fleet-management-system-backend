namespace business_logic.Settings
{
    public class CityGeocodingSettings
    {
        public const string SectionName = "CityGeocodingSettings";

        public string BaseUrl { get; set; } = string.Empty;
        public string StoredNamesLanguageCode { get; set; } = string.Empty;
        public int RequestTimeoutInSeconds { get; set; }
    }
}
