using business_logic.Constants;
using System.Globalization;

namespace business_logic.Helpers
{
    public static class CurrentUiLanguageResolver
    {
        public static string GetCurrentSupportedLanguageCode()
        {
            var currentUiLanguageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (!SupportedLanguages.All.Contains(currentUiLanguageCode))
            {
                throw new InvalidOperationException(
                    $"Current UI culture '{CultureInfo.CurrentUICulture.Name}' is not one of the supported languages '{string.Join(", ", SupportedLanguages.All)}'.");
            }

            return currentUiLanguageCode;
        }
    }
}
