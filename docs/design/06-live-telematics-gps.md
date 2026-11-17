# Телематика та GPS у реальному часі

Статус: проєкт, не реалізовано. Розмір: **L**.

## Проблема

Диспетчер бачить лише плановий час рейсу і вручну перемикає статуси. Він не знає, де зараз авто, чи воно запізнюється і коли насправді приїхало. Позиція авто потрібна також для автоматичного пробігу ([01-mileage-and-maintenance.md](01-mileage-and-maintenance.md)) і для майбутньої перевірки безпеки маршрутів.

Рекомендований підхід: **не** приймати сирі протоколи GPS-трекерів у цьому API, а інтегруватися з телематичною платформою (Traccar як self-hosted, або Wialon чи API виробника трекерів). Платформа приймає дані від пристроїв, а система отримує нормалізовані позиції через webhook або опитування.

## Сутності та зв'язки

| Сутність | Поля | Примітки |
|---|---|---|
| `TelematicsDevice` | `Id`, `AutoId`, `TelematicsProvider` (enum), `ExternalDeviceId`, `InstalledAtUtc`, `IsDeactivated`, `DepotId` | Унікальний `(TelematicsProvider, ExternalDeviceId)`. Одне активне устаткування на авто. |
| `VehiclePosition` | `Id` (bigint), `AutoId`, `RecordedAtUtc`, `ReceivedAtUtc`, `Latitude` (decimal 9,6), `Longitude` (decimal 9,6), `SpeedKmh`, `HeadingDegrees`, `OdometerKm` (int?), `IsIgnitionOn` (bool?), `DepotId` | Історія точок. Унікальний `(AutoId, RecordedAtUtc)` проти дублікатів. Окрема таблиця з політикою зберігання (наприклад, 90 днів) і поділом за місяцями. |
| `LatestVehiclePosition` | `AutoId` (PK), ті самі поля | Один рядок на авто для швидкої карти. Оновлюється разом із вставкою історії. |
| `Location` (змінюється) | + `Latitude`, `Longitude` (decimal?), + `GeofenceRadiusMetres` (int?) | Геозона для визначення прибуття й відправлення. |

Логіка:
- **Пропозиції статусів**: виїзд з геозони початкової локації пропонує `InProgress`, в'їзд у геозону кінцевої — `Completed`. Перша версія лише пропонує, диспетчер підтверджує.
- **ETA**: за поточною позицією через сервіс маршрутизації (Google Directions вже використовується для карти; це окрема ліцензія й вартість).
- **Live-оновлення**: SignalR-хаб `/hubs/vehiclePositions`; підключення додається до груп своїх депо, тож фільтр депо діє і для push-оновлень.

## Ендпоінти

| Метод і маршрут | Модель запиту | Модель відповіді |
|---|---|---|
| `POST /api/v1/Telematics/receiveTelematicsPositions` | `ReceiveTelematicsPositionsRequestModel` (пакет точок) | `204`. Без JWT: автентифікація HMAC-підписом (`X-Telematics-Signature`) і секретом із конфігурації |
| `GET /api/v1/Telematics/getLatestVehiclePositions` | `GetLatestVehiclePositionsRequestModel` (депо) | `GetLatestVehiclePositionsResponseModel` |
| `GET /api/v1/Telematics/getVehicleTrack` | `GetVehicleTrackRequestModel` (`AutoId`, `FromUtc`, `ToUtc`) | `GetVehicleTrackResponseModel` (спрощений трек) |
| `POST /api/v1/TelematicsDevice/registerTelematicsDevice` | `RegisterTelematicsDeviceRequestModel` | `RegisterTelematicsDeviceResponseModel` |
| `POST /api/v1/TelematicsDevice/deactivateTelematicsDevice/{id}` | — | `204` |
| `GET /api/v1/TelematicsDevice/getTelematicsDevicesPagedList` | `GetTelematicsDevicesPagedListRequestModel` | `GetTelematicsDevicesPagedListResponseModel` |

## Міграції та ризики даних

- Обсяг: точка кожні 30 с × 100 авто ≈ 288 000 рядків на добу. Потрібні кластерний індекс `(AutoId, RecordedAtUtc)`, поділ на партиції або columnstore у SQL Server і фонове очищення за політикою зберігання. Розглянути окреме сховище часових рядів.
- Час пристрою може відставати чи поспішати: зберігати і `RecordedAtUtc`, і `ReceivedAtUtc`, відкидати точки з майбутнього.
- Webhook — публічний ендпоінт без JWT. Потрібні перевірка підпису, захист від повторів (мітка часу у вікні 5 хв), обмеження частоти й розміру. Секрет обов'язковий у production.
- **Приватність**: відстеження водіїв — обробка персональних даних. Потрібні правова підстава, повідомлення працівників і, можливо, вимкнення відстеження поза робочим часом. Доступ до треку — лише з фільтром депо.
- Нові колонки в `Locations` — nullable. Геозони працюватимуть лише після заповнення координат.

## Екрани

- Сторінка «Карта»: усі авто депо з live-позиціями, статусом рейсу, швидкістю й часом останньої точки; позначка «немає зв'язку» після N хвилин.
- Картка маршруту: поточна позиція, ETA, пропозиція змінити статус.
- Модальне вікно авто: «Трек за період» з програванням.
- Адмін: реєстр пристроїв.
- Довідник локацій: вибір координат на карті.

## Відкриті питання

1. Які трекери вже встановлено (чи є вони взагалі) і яка платформа ними керує?
2. Потрібна частота оновлення і скільки зберігати історію?
3. Хто бачить live-позицію: лише диспетчери депо чи й адміністратор усіх депо?
4. Чи можна відстежувати авто поза рейсом, у неробочий час?
5. Чи телефон водія може бути джерелом GPS замість трекера (потрібні роль водія та мобільний застосунок)?
