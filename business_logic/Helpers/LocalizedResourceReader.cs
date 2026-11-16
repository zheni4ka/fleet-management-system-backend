using System.Globalization;
using System.Resources;

namespace business_logic.Helpers
{
    public static class LocalizedResourceReader
    {
        public static string GetRequiredLocalizedResource(ResourceManager resourceManager, string resourceKey)
        {
            return resourceManager.GetString(resourceKey, CultureInfo.CurrentUICulture)
                ?? throw new InvalidOperationException($"Resource '{resourceKey}' is missing from '{resourceManager.BaseName}'.");
        }
    }
}
