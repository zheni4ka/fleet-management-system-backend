using business_logic.Constants;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace business_logic_tests.Infrastructure
{
    public static class TestCultureInitializer
    {
        [ModuleInitializer]
        public static void UseDefaultBusinessUiCulture()
        {
            var defaultBusinessUiCulture = new CultureInfo(SupportedLanguages.Default);
            CultureInfo.DefaultThreadCurrentUICulture = defaultBusinessUiCulture;
            CultureInfo.CurrentUICulture = defaultBusinessUiCulture;
        }
    }
}
