namespace business_logic.Constants
{
    public static class ExceptionMessages
    {
        public const string EntityNotFoundTemplate = "{0} з ідентифікатором '{1}' не знайдено.";
        public const string AutoNumberAlreadyExistsTemplate = "Автомобіль з номером '{0}' вже існує.";
        public const string DriverLicenseNumberAlreadyExistsTemplate = "Водій з посвідченням '{0}' вже існує.";
        public const string AutoUnderMaintenanceTemplate = "Автомобіль з ідентифікатором '{0}' перебуває на техобслуговуванні.";
        public const string AutoAssignmentConflictTemplate = "Автомобіль (ID {0}) вже призначено на маршрут #{1}, який перетинається з інтервалом {2}.";
        public const string DriverAssignmentConflictTemplate = "Водія (ID {0}) вже призначено на маршрут #{1}, який перетинається з інтервалом {2}.";
        public const string MaintenanceAssignmentConflictTemplate = "Для автомобіля (ID {0}) заплановано техобслуговування #{1}, яке припадає на інтервал {2}.";
        public const string SortFieldNotSupportedTemplate = "Поле сортування '{0}' не підтримується.";
        public const string WrongUsernameOrPassword = "Неправильне ім'я користувача або пароль.";
        public const string UsernameAlreadyExistsTemplate = "Користувач з логіном '{0}' вже існує.";
        public const string EmailAlreadyExistsTemplate = "Користувач з email '{0}' вже існує.";
        public const string UserCreationFailedTemplate = "Не вдалося створити користувача '{0}': {1}";
        public const string RoleAssignmentFailedTemplate = "Не вдалося призначити роль '{0}' користувачу '{1}': {2}";
        public const string AccountLockedOut = "Обліковий запис тимчасово заблоковано через забагато невдалих спроб входу. Спробуйте пізніше.";
        public const string PasswordChangeRequired = "Потрібно змінити тимчасовий пароль перед входом у систему.";
        public const string RefreshTokenInvalid = "Сесія недійсна або завершилась. Увійдіть у систему повторно.";
        public const string PasswordChangeFailedTemplate = "Не вдалося змінити пароль користувача '{0}': {1}";
        public const string PasswordResetFailedTemplate = "Не вдалося скинути пароль користувача з ідентифікатором '{0}': {1}";
        public const string ExternalIdentityNotConfigured = "Вхід через корпоративний обліковий запис не налаштовано.";
        public const string ExternalIdentityTokenInvalid = "Не вдалося підтвердити вхід через корпоративний обліковий запис.";
        public const string ExternalIdentityHasNoMappedGroupTemplate = "Корпоративний обліковий запис '{0}' не входить до жодної групи з доступом до системи.";
        public const string ExternalIdentityUsernameMissingTemplate = "Корпоративний обліковий запис '{0}' не містить імені користувача чи email.";
        public const string ConcurrencyConflictTemplate = "{0} з ідентифікатором '{1}' змінив інший користувач. Оновіть дані та повторіть дію.";
        public const string ApplicationLockNotAcquiredTemplate = "Інший запит саме змінює призначення для '{0}'. Спробуйте ще раз.";
        public const string ApplicationLockFailedTemplate = "Не вдалося отримати блокування '{0}': код {1}.";
        public const string DepotNameAlreadyExistsTemplate = "Депо з назвою '{0}' вже існує.";
        public const string DepotInUseTemplate = "Неможливо видалити депо з ідентифікатором '{0}', оскільки до нього належать автомобілі, водії, маршрути або користувачі.";
        public const string DriverDepotMismatchTemplate = "Водій (ID {0}) належить до депо #{1}, а автомобіль (ID {2}) — до депо #{3}. Маршрут можна призначити лише в межах одного депо.";
        public const string AutoDepotChangeForbiddenTemplate = "Неможливо змінити депо автомобіля з ідентифікатором '{0}', оскільки він має маршрути.";
        public const string DriverDepotChangeForbiddenTemplate = "Неможливо змінити депо водія з ідентифікатором '{0}', оскільки його призначено на маршрути.";
        public const string AutoArchivedTemplate = "Автомобіль з ідентифікатором '{0}' перебуває в архіві.";
        public const string DriverArchivedTemplate = "Водій з ідентифікатором '{0}' перебуває в архіві.";
        public const string LocationsArchivedTemplate = "Локації з ідентифікаторами '{0}' перебувають в архіві.";
        public const string AutoHasActiveRoutesTemplate = "Неможливо видалити автомобіль з ідентифікатором '{0}', оскільки він має заплановані або активні маршрути.";
        public const string DriverHasActiveRoutesTemplate = "Неможливо видалити водія з ідентифікатором '{0}', оскільки його призначено на заплановані або активні маршрути.";
        public const string LocationHasActiveRoutesTemplate = "Неможливо видалити локацію з ідентифікатором '{0}', оскільки вона використовується в запланованих або активних маршрутах.";
        public const string EntityNotArchivedTemplate = "{0} з ідентифікатором '{1}' не перебуває в архіві.";
        public const string AccountDeactivated = "Обліковий запис деактивовано. Зверніться до адміністратора.";
        public const string OwnAccountDeactivationForbidden = "Неможливо деактивувати власний обліковий запис.";
        public const string UserNotDeactivatedTemplate = "Користувач з ідентифікатором '{0}' не деактивований.";
        public const string UserDeactivationFailedTemplate = "Не вдалося змінити стан облікового запису користувача з ідентифікатором '{0}': {1}";
        public const string TooManyRequests = "Забагато запитів. Зачекайте трохи та спробуйте знову.";
        public const string UnexpectedError = "Сталася неочікувана помилка.";
        public const string ValidationFailed = "Одна або кілька перевірок даних не пройдені.";
    }
}
