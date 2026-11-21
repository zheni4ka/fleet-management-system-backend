namespace business_logic.Constants
{
    public static class DriverConstraints
    {
        public const int NameMaxLength = 100;
        public const int SurnameMaxLength = 100;
        public const int PatronymicMaxLength = 100;
        public const int PhoneNumberMaxLength = 16;
        public const string PhoneNumberPattern = @"^\+[1-9][0-9]{7,14}$";
    }
}
