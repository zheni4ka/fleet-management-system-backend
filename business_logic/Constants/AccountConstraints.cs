namespace business_logic.Constants
{
    public static class AccountConstraints
    {
        public const int UsernameMaxLength = 50;
        public const int EmailMaxLength = 256;
        public const int PasswordMinLength = 12;
        public const int PasswordMaxLength = 100;
        public const string PasswordDigitPattern = "[0-9]";
        public const string PasswordUppercaseLetterPattern = "[A-Z]";
        public const string PasswordLowercaseLetterPattern = "[a-z]";
        public const int LockoutMaxFailedAccessAttempts = 5;
        public const int LockoutDurationInMinutes = 15;
        public const int RefreshTokenByteLength = 32;
        public const int RefreshTokenHashLength = 64;
        public const int TemporaryPasswordLength = 16;
        public const string TemporaryPasswordUppercaseLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        public const string TemporaryPasswordLowercaseLetters = "abcdefghijkmnopqrstuvwxyz";
        public const string TemporaryPasswordDigits = "23456789";
    }
}
