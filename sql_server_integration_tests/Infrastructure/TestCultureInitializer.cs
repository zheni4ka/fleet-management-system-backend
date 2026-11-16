using business_logic.Constants;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace sql_server_integration_tests.Infrastructure
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
