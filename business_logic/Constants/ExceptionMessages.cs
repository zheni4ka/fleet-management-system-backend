using business_logic.Helpers;
using System.Resources;

namespace business_logic.Constants
{
    public static class ExceptionMessages
    {
        private static readonly ResourceManager ExceptionMessagesResourceManager = new("business_logic.Resources.ExceptionMessageResources", typeof(ExceptionMessages).Assembly);

        public static string EntityNotFoundTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(EntityNotFoundTemplate));
        public static string AutoNumberAlreadyExistsTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(AutoNumberAlreadyExistsTemplate));
        public static string DriverLicenseNumberAlreadyExistsTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(DriverLicenseNumberAlreadyExistsTemplate));
        public static string AutoUnderMaintenanceTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(AutoUnderMaintenanceTemplate));
        public static string AutoAssignmentConflictTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(AutoAssignmentConflictTemplate));
        public static string DriverAssignmentConflictTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(DriverAssignmentConflictTemplate));
        public static string MaintenanceAssignmentConflictTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(MaintenanceAssignmentConflictTemplate));
        public static string SortFieldNotSupportedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(SortFieldNotSupportedTemplate));
        public static string WrongUsernameOrPassword => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(WrongUsernameOrPassword));
        public static string UsernameAlreadyExistsTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(UsernameAlreadyExistsTemplate));
        public static string EmailAlreadyExistsTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(EmailAlreadyExistsTemplate));
        public static string UserCreationFailedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(UserCreationFailedTemplate));
        public static string RoleAssignmentFailedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(RoleAssignmentFailedTemplate));
        public static string AccountLockedOut => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(AccountLockedOut));
        public static string PasswordChangeRequired => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(PasswordChangeRequired));
        public static string RefreshTokenInvalid => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(RefreshTokenInvalid));
        public static string PasswordChangeFailedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(PasswordChangeFailedTemplate));
        public static string PasswordResetFailedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(PasswordResetFailedTemplate));
        public static string ExternalIdentityNotConfigured => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(ExternalIdentityNotConfigured));
        public static string ExternalIdentityTokenInvalid => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(ExternalIdentityTokenInvalid));
        public static string ExternalIdentityHasNoMappedGroupTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(ExternalIdentityHasNoMappedGroupTemplate));
        public static string ExternalIdentityUsernameMissingTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(ExternalIdentityUsernameMissingTemplate));
        public static string ConcurrencyConflictTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(ConcurrencyConflictTemplate));
        public static string ApplicationLockNotAcquiredTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(ApplicationLockNotAcquiredTemplate));
        public static string ApplicationLockFailedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(ApplicationLockFailedTemplate));
        public static string DepotNameAlreadyExistsTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(DepotNameAlreadyExistsTemplate));
        public static string DepotInUseTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(DepotInUseTemplate));
        public static string DriverDepotMismatchTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(DriverDepotMismatchTemplate));
        public static string AutoDepotChangeForbiddenTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(AutoDepotChangeForbiddenTemplate));
        public static string DriverDepotChangeForbiddenTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(DriverDepotChangeForbiddenTemplate));
        public static string AutoArchivedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(AutoArchivedTemplate));
        public static string DriverArchivedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(DriverArchivedTemplate));
        public static string LocationsArchivedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(LocationsArchivedTemplate));
        public static string AutoHasActiveRoutesTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(AutoHasActiveRoutesTemplate));
        public static string DriverHasActiveRoutesTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(DriverHasActiveRoutesTemplate));
        public static string LocationHasActiveRoutesTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(LocationHasActiveRoutesTemplate));
        public static string EntityNotArchivedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(EntityNotArchivedTemplate));
        public static string AccountDeactivated => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(AccountDeactivated));
        public static string OwnAccountDeactivationForbidden => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(OwnAccountDeactivationForbidden));
        public static string UserNotDeactivatedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(UserNotDeactivatedTemplate));
        public static string UserDeactivationFailedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(UserDeactivationFailedTemplate));
        public static string PreferredLanguageUpdateFailedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(PreferredLanguageUpdateFailedTemplate));
        public static string TooManyRequests => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(TooManyRequests));
        public static string UnexpectedError => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(UnexpectedError));
        public static string ValidationFailed => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(ValidationFailed));
        public static string LocationGeoNamesIdAlreadyExistsTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(LocationGeoNamesIdAlreadyExistsTemplate));
        public static string CityGeocodingServiceUnavailable => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(CityGeocodingServiceUnavailable));
        public static string CityGeocodingCandidateNotFoundTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(CityGeocodingCandidateNotFoundTemplate));
        public static string CityGeocodingTimeZoneUnsupportedTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(CityGeocodingTimeZoneUnsupportedTemplate));
        public static string RouteStatusTransitionForbiddenTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(RouteStatusTransitionForbiddenTemplate));
        public static string FinalRouteAssignmentChangeForbiddenTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(FinalRouteAssignmentChangeForbiddenTemplate));
        public static string RouteRescheduleRequiresPlannedStatusTemplate => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(RouteRescheduleRequiresPlannedStatusTemplate));
        public static string AddressGeocodingServiceUnavailable => LocalizedResourceReader.GetRequiredLocalizedResource(ExceptionMessagesResourceManager, nameof(AddressGeocodingServiceUnavailable));
    }
}
