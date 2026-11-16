using business_logic.Constants;
using business_logic.Entities;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Helpers
{
    public static class EntityDisplayNameResolver
    {
        public static string GetEntityDisplayName(Type entityType)
        {
            return entityType switch
            {
                _ when entityType == typeof(Auto) => EntityDisplayNames.Auto,
                _ when entityType == typeof(Driver) => EntityDisplayNames.Driver,
                _ when entityType == typeof(Location) => EntityDisplayNames.Location,
                _ when entityType == typeof(Route) => EntityDisplayNames.Route,
                _ when entityType == typeof(AutoMaintenance) => EntityDisplayNames.AutoMaintenance,
                _ when entityType == typeof(IdentityUser) => EntityDisplayNames.User,
                _ => throw new InvalidOperationException($"Entity type '{entityType.Name}' has no display name.")
            };
        }
    }
}
