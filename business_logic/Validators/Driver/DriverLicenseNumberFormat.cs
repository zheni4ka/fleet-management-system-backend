namespace business_logic.Validators
{
    internal static class DriverLicenseNumberFormat
    {
        public const string Pattern = @"^[A-Z]{3}\s\d{6}$";

        public const string InvalidFormatMessage = "Посвідчення водія має формат ABC 123456.";
    }
}
