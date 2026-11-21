namespace data_access.data
{
    public static class DemoDataCatalog
    {
        public const int RandomSeed = 2026;
        public const int HistoryDaysCount = 60;
        public const int PlanningHorizonDaysCount = 7;

        public static readonly IReadOnlyList<(string Name, int HomeLocationId)> Depots =
        [
            ("Київське депо", 1),
            ("Львівське депо", 2),
            ("Одеське депо", 3)
        ];

        public static readonly IReadOnlyList<(string UserName, string Email, int DepotIndex)> Dispatchers =
        [
            ("dispatcher.kyiv", "dispatcher.kyiv@fleet.example", 0),
            ("dispatcher.lviv", "dispatcher.lviv@fleet.example", 1),
            ("dispatcher.odesa", "dispatcher.odesa@fleet.example", 2)
        ];

        public static readonly IReadOnlyList<(string Mark, string Model, string Color, string Number, double Capacity, int DepotIndex)> Autos =
        [
            ("Volvo", "FH16", "Білий", "АА1001ВК", 20, 0),
            ("Scania", "R450", "Синій", "АА2034КМ", 18, 0),
            ("MAN", "TGX", "Сірий", "АІ3450ВС", 22, 0),
            ("DAF", "XF", "Червоний", "АА4521НЕ", 19, 0),
            ("Mercedes-Benz", "Actros", "Чорний", "ВС1287АК", 24, 1),
            ("Renault", "T480", "Білий", "ВС5610ІР", 18, 1),
            ("Iveco", "S-Way", "Зелений", "ВС7742МО", 16, 1),
            ("Volvo", "FM", "Сірий", "ВС9003ТА", 15, 1),
            ("Scania", "S500", "Жовтий", "ВН2210ЕК", 21, 2),
            ("MAN", "TGS", "Синій", "ВН4378СА", 17, 2),
            ("DAF", "CF", "Білий", "ВН6150НІ", 14, 2),
            ("Mercedes-Benz", "Arocs", "Чорний", "ВН8825ВМ", 23, 2)
        ];

        public static readonly IReadOnlyList<(string Name, string Surname, string Patronymic, string LicenseNumber, int DepotIndex)> Drivers =
        [
            ("Тарас", "Шевченко", "Григорович", "KYV 100001", 0),
            ("Олена", "Коваленко", "Петрівна", "KYV 100002", 0),
            ("Андрій", "Бондаренко", "Іванович", "KYV 100003", 0),
            ("Ігор", "Мельник", "Васильович", "KYV 100004", 0),
            ("Марія", "Ткаченко", "Олегівна", "KYV 100005", 0),
            ("Остап", "Гнатюк", "Богданович", "LVV 200001", 1),
            ("Соломія", "Кравчук", "Андріївна", "LVV 200002", 1),
            ("Роман", "Савчук", "Миколайович", "LVV 200003", 1),
            ("Мар'яна", "Бойко", "Степанівна", "LVV 200004", 1),
            ("Юрій", "Левицький", "Романович", "LVV 200005", 1),
            ("Дмитро", "Поліщук", "Сергійович", "ODS 300001", 2),
            ("Наталія", "Мороз", "Віталіївна", "ODS 300002", 2),
            ("Віктор", "Лисенко", "Анатолійович", "ODS 300003", 2),
            ("Сергій", "Руденко", "Олександрович", "ODS 300004", 2),
            ("Анна-Марія", "Захарченко", "Юріївна", "ODS 300005", 2)
        ];

        public static readonly IReadOnlyList<(int LocationId, double Latitude, double Longitude)> CityCoordinates =
        [
            (1, 50.4501, 30.5234),
            (2, 49.8397, 24.0297),
            (3, 46.4825, 30.7233),
            (4, 49.9935, 36.2304),
            (5, 48.4647, 35.0462),
            (6, 47.8388, 35.1396),
            (7, 48.9226, 24.7111),
            (8, 49.5535, 25.5948),
            (9, 48.2915, 25.9403),
            (10, 49.2331, 28.4682),
            (11, 50.6199, 26.2516),
            (12, 50.7472, 25.3254),
            (13, 49.5883, 34.5514),
            (14, 51.4982, 31.2893),
            (15, 49.4444, 32.0598),
            (16, 49.4229, 26.9871),
            (17, 50.2547, 28.6587),
            (18, 48.6208, 22.2879),
            (19, 46.9750, 31.9946),
            (20, 46.6354, 32.6169),
            (21, 50.9077, 34.7981),
            (22, 48.5079, 32.2623),
            (24, 52.2297, 21.0122),
            (25, 50.0647, 19.9450),
            (26, 51.1079, 17.0385)
        ];

        public static readonly IReadOnlyList<(int AutoIndex, string Name, string Description, int StartDayOffset, int StartHour, int? PlannedEndDayOffset, int PlannedEndHour, bool IsCompleted)> Maintenances =
        [
            (0, "ТО-1", "Планова заміна мастила й фільтрів", -45, 9, -44, 18, true),
            (4, "Ремонт гальм", "Знос гальмівних колодок задньої осі", -20, 8, -18, 17, true),
            (7, "Діагностика двигуна", "Нестабільні оберти на холостому ходу", -2, 8, null, 0, false),
            (10, "ТО-2", "Планове технічне обслуговування за пробігом", 3, 9, 4, 18, false)
        ];
    }
}
