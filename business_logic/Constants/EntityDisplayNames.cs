using business_logic.Helpers;
using System.Resources;

namespace business_logic.Constants
{
    public static class EntityDisplayNames
    {
        private static readonly ResourceManager EntityDisplayNamesResourceManager = new("business_logic.Resources.EntityDisplayNameResources", typeof(EntityDisplayNames).Assembly);

        public static string Auto => LocalizedResourceReader.GetRequiredLocalizedResource(EntityDisplayNamesResourceManager, nameof(Auto));
        public static string Driver => LocalizedResourceReader.GetRequiredLocalizedResource(EntityDisplayNamesResourceManager, nameof(Driver));
        public static string Location => LocalizedResourceReader.GetRequiredLocalizedResource(EntityDisplayNamesResourceManager, nameof(Location));
        public static string Route => LocalizedResourceReader.GetRequiredLocalizedResource(EntityDisplayNamesResourceManager, nameof(Route));
        public static string AutoMaintenance => LocalizedResourceReader.GetRequiredLocalizedResource(EntityDisplayNamesResourceManager, nameof(AutoMaintenance));
        public static string User => LocalizedResourceReader.GetRequiredLocalizedResource(EntityDisplayNamesResourceManager, nameof(User));
        public static string Depot => LocalizedResourceReader.GetRequiredLocalizedResource(EntityDisplayNamesResourceManager, nameof(Depot));
    }
}
