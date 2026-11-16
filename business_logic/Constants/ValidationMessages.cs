namespace business_logic.Constants
{
    public static class ValidationMessages
    {
        public const string RequiredField = "Поле «{PropertyName}» є обов'язковим.";
        public const string MaximumLengthExceeded = "Поле «{PropertyName}» не може перевищувати {MaxLength} символів.";
        public const string PositiveNumberRequired = "Поле «{PropertyName}» має бути більшим за 0.";
        public const string UnsupportedValue = "Поле «{PropertyName}» має недопустиме значення.";
    }
}
