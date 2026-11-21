namespace business_logic.Constants
{
    public static class AddressGeocodingConstraints
    {
        public const int SearchTextMinLength = 3;
        public const int SearchTextMaxLength = 200;
        public const int SearchResultsMaxCount = 8;
        public const int ProviderResultsOverfetchFactor = 3;
        public const double SearchAreaLatitudeRadiusInDegrees = 0.25;
        public const double SearchAreaLongitudeRadiusInDegrees = 0.35;
        public const int AddressLineMaxLength = 300;
        public const int CoordinateFractionDigits = 6;
        public const double MinimumLatitude = -90;
        public const double MaximumLatitude = 90;
        public const double MinimumLongitude = -180;
        public const double MaximumLongitude = 180;
    }
}
