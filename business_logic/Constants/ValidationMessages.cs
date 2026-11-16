namespace business_logic.Constants
{
    public static class ValidationMessages
    {
        public const string RequiredField = "Поле «{PropertyName}» є обов'язковим.";
        public const string MaximumLengthExceeded = "Поле «{PropertyName}» не може перевищувати {MaxLength} символів.";
        public const string PositiveNumberRequired = "Поле «{PropertyName}» має бути більшим за 0.";
        public const string UnsupportedValue = "Поле «{PropertyName}» має недопустиме значення.";
        public const string MinimumLengthRequired = "Поле «{PropertyName}» має містити щонайменше {MinLength} символів.";
        public const string PasswordDigitRequired = "Поле «{PropertyName}» має містити щонайменше одну цифру.";
        public const string PasswordUppercaseLetterRequired = "Поле «{PropertyName}» має містити щонайменше одну велику латинську літеру.";
        public const string PasswordLowercaseLetterRequired = "Поле «{PropertyName}» має містити щонайменше одну малу латинську літеру.";
        public const string NewPasswordMustDifferFromCurrent = "Новий пароль має відрізнятися від поточного.";
        public const string DispatcherDepotsRequired = "Диспетчеру потрібно призначити щонайменше одне депо.";
        public const string AdminDepotsForbidden = "Адміністратор має доступ до всіх депо, тому депо йому не призначаються.";
        public const string UnknownTimeZoneId = "Поле «{PropertyName}» має бути ідентифікатором часового поясу IANA, наприклад Europe/Kyiv.";
        public const string UtcTimeRequired = "Поле «{PropertyName}» має бути часом UTC (із суфіксом Z).";
    }
}
