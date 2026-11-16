namespace business_logic.Constants
{
    public static class SupportedLanguages
    {
        public const string Ukrainian = "uk";
        public const string English = "en";
        public const string Default = Ukrainian;

        public static readonly IReadOnlyList<string> All = [Ukrainian, English];
    }
}
