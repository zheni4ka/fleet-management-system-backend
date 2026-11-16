namespace business_logic.Constants
{
    public static class ExceptionMessages
    {
        public const string EntityNotFoundTemplate = "{0} з ідентифікатором '{1}' не знайдено.";
        public const string AutoNumberAlreadyExistsTemplate = "Автомобіль з номером '{0}' вже існує.";
        public const string DriverLicenseNumberAlreadyExistsTemplate = "Водій з посвідченням '{0}' вже існує.";
        public const string AutoUsedInRoutesTemplate = "Неможливо видалити автомобіль з ідентифікатором '{0}', оскільки він використовується в маршрутах.";
        public const string DriverAssignedToRoutesTemplate = "Неможливо видалити водія з ідентифікатором '{0}', оскільки його призначено на маршрути.";
        public const string LocationUsedInRoutesTemplate = "Неможливо видалити локацію з ідентифікатором '{0}', оскільки вона використовується в маршрутах.";
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
        public const string UserDeletionFailedTemplate = "Не вдалося видалити користувача з ідентифікатором '{0}': {1}";
        public const string OwnAccountDeletionForbidden = "Неможливо видалити власний обліковий запис.";
        public const string UnexpectedError = "Сталася неочікувана помилка.";
        public const string ValidationFailed = "Одна або кілька перевірок даних не пройдені.";
    }
}
