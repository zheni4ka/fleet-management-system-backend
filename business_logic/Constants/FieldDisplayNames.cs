using business_logic.Helpers;
using System.Resources;

namespace business_logic.Constants
{
    public static class FieldDisplayNames
    {
        private static readonly ResourceManager FieldDisplayNamesResourceManager = new("business_logic.Resources.FieldDisplayNameResources", typeof(FieldDisplayNames).Assembly);

        public static string Depot => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Depot));
        public static string Id => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Id));
        public static string RowVersion => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(RowVersion));
        public static string Auto => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Auto));
        public static string RefreshToken => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(RefreshToken));
        public static string Status => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Status));
        public static string StartLocation => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(StartLocation));
        public static string DestinationLocation => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(DestinationLocation));
        public static string Password => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Password));
        public static string City => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(City));
        public static string LoginOrEmail => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(LoginOrEmail));
        public static string Driver => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Driver));
        public static string UserId => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(UserId));
        public static string DepartureTime => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(DepartureTime));
        public static string ArrivalTime => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(ArrivalTime));
        public static string IdentityToken => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(IdentityToken));
        public static string Nonce => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Nonce));
        public static string MaintenanceWorkType => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(MaintenanceWorkType));
        public static string MaintenanceProblemDescription => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(MaintenanceProblemDescription));
        public static string Role => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Role));
        public static string Name => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Name));
        public static string Surname => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Surname));
        public static string Patronymic => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Patronymic));
        public static string DriverLicenseNumber => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(DriverLicenseNumber));
        public static string CurrentPassword => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(CurrentPassword));
        public static string NewPassword => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(NewPassword));
        public static string AutoNumber => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(AutoNumber));
        public static string DepotName => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(DepotName));
        public static string Capacity => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Capacity));
        public static string Language => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Language));
        public static string Mark => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Mark));
        public static string Model => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Model));
        public static string Color => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Color));
        public static string Username => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Username));
        public static string Email => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Email));
        public static string AuditAction => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(AuditAction));
        public static string AutoStatus => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(AutoStatus));
        public static string RouteStatus => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(RouteStatus));
        public static string Page => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Page));
        public static string PageSize => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(PageSize));
        public static string SortField => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(SortField));
        public static string SortDirection => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(SortDirection));
        public static string SearchQuery => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(SearchQuery));
        public static string Country => LocalizedResourceReader.GetRequiredLocalizedResource(FieldDisplayNamesResourceManager, nameof(Country));
    }
}
