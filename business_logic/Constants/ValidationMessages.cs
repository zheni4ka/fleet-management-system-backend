using business_logic.Helpers;
using System.Resources;

namespace business_logic.Constants
{
    public static class ValidationMessages
    {
        private static readonly ResourceManager ValidationMessagesResourceManager = new("business_logic.Resources.ValidationMessageResources", typeof(ValidationMessages).Assembly);

        public static string RequiredField => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(RequiredField));
        public static string MaximumLengthExceeded => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(MaximumLengthExceeded));
        public static string PositiveNumberRequired => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(PositiveNumberRequired));
        public static string UnsupportedValue => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(UnsupportedValue));
        public static string MinimumLengthRequired => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(MinimumLengthRequired));
        public static string PasswordDigitRequired => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(PasswordDigitRequired));
        public static string PasswordUppercaseLetterRequired => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(PasswordUppercaseLetterRequired));
        public static string PasswordLowercaseLetterRequired => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(PasswordLowercaseLetterRequired));
        public static string NewPasswordMustDifferFromCurrent => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(NewPasswordMustDifferFromCurrent));
        public static string DispatcherDepotsRequired => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(DispatcherDepotsRequired));
        public static string AdminDepotsForbidden => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(AdminDepotsForbidden));
        public static string UnknownTimeZoneId => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(UnknownTimeZoneId));
        public static string UtcTimeRequired => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(UtcTimeRequired));
        public static string MaintenancePlannedEndMustFollowStart => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(MaintenancePlannedEndMustFollowStart));
        public static string RouteTimesMustBeProvidedTogether => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(RouteTimesMustBeProvidedTogether));
        public static string DepartureMustPrecedeArrival => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(DepartureMustPrecedeArrival));
        public static string MaintenanceStartMustNotBeInPast => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(MaintenanceStartMustNotBeInPast));
        public static string EmailAddressInvalid => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(EmailAddressInvalid));
        public static string StartAndDestinationLocationsMustDiffer => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(StartAndDestinationLocationsMustDiffer));
        public static string DateRangeEndBeforeStart => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(DateRangeEndBeforeStart));
        public static string PeriodEndBeforeStart => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(PeriodEndBeforeStart));
        public static string PeriodLengthExceededTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(PeriodLengthExceededTemplate));
        public static string NumberOutOfRange => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(NumberOutOfRange));
        public static string UnsupportedRoleTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(UnsupportedRoleTemplate));
        public static string AutoNumberFormatInvalid => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(AutoNumberFormatInvalid));
        public static string DriverLicenseNumberFormatInvalid => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(DriverLicenseNumberFormatInvalid));
        public static string DriverNameFormatInvalid => LocalizedResourceReader.GetRequiredLocalizedResource(ValidationMessagesResourceManager, nameof(DriverNameFormatInvalid));
    }
}
