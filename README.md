# Fleet Management System — backend

REST API системи обліку автопарку й логістики маршрутів (ASP.NET Core, .NET 10, EF Core, SQL Server).

## Запуск локально

1. Відновити локальні інструменти (зафіксована версія `dotnet-ef`):
   ```bash
   dotnet tool restore
   ```
2. Застосувати міграції до LocalDB (лише для розробки; на серверах міграції застосовує bundle, див. нижче):
   ```bash
   dotnet ef database update --project data_access --startup-project transport-logistic-management-2026
   ```
3. Налаштувати секрети. Їх немає в `appsettings.json`, вони зберігаються в user-secrets:
   ```bash
   dotnet user-secrets set "JwtSettings:Secret" "<випадковий рядок, не менше 64 символів>" --project transport-logistic-management-2026
   ```
   Для нової БД, де ще немає адміністратора, потрібен також пароль адміністратора за замовчуванням (логін `admin`, щонайменше 12 символів із великою й малою латинськими літерами та цифрою):
   ```bash
   dotnet user-secrets set "SeedAdmin:Password" "<пароль>" --project transport-logistic-management-2026
   ```
4. Запустити API (http://localhost:5196, документація — `/scalar/v1`):
   ```bash
   dotnet run --project transport-logistic-management-2026 --launch-profile http
   ```

Поза середовищем Development значення передаються змінними середовища (`Section__Key`), наприклад `JwtSettings__Secret`, `SeedAdmin__Password`, `CorsSettings__AllowedOrigins__0`.

### Демонстраційні дані

Команда `seed-demo` заповнює **порожній** автопарк даними для показу: 3 депо, 12 авто, 15 водіїв, координати 25 міст, близько 150 маршрутів за останні 60 днів і наступний тиждень у різних статусах (із фактичним часом виконаних рейсів), 4 записи ТО (завершені, поточне без кінця й заплановане) та журнал дій зі змінами «було → стало». Додаються диспетчери `dispatcher.kyiv`, `dispatcher.lviv`, `dispatcher.odesa` — кожен зі своїм депо. Дані детерміновані (фіксоване зерно генератора) і прив'язані до поточної дати.

```bash
dotnet user-secrets set "DemoSeed:DispatcherPassword" "<пароль диспетчерів>" --project transport-logistic-management-2026
dotnet run --project transport-logistic-management-2026 --launch-profile http -- seed-demo
```

Команда працює лише в середовищі Development, вимагає застосованих міграцій і відмовляється запускатися, якщо в БД уже є хоча б одне авто. Для чистого показу зручно створити окрему БД (`dotnet ef database update --connection "...;Database=FleetDemo;..."`) і передати її рядок підключення через `ConnectionStrings__connectionString`.

### Docker Compose

`docker-compose.yml` піднімає SQL Server, застосовує міграції bundle-ом, запускає API (порт 5196) і фронтенд (порт 3000). Скопіюйте `docker.env.example` у `.env`, заповніть секрети й шлях до репозиторію фронтенду, потім:

```bash
docker compose up --build
```

## Версіонування API

- Усі маршрути мають префікс `api/v{версія}/`, поточна версія — `v1` (наприклад, `POST /api/v1/Account/login`). Маршрути без версії не обслуговуються.
- Назви дій лишаються явними й у camelCase (`getAutosPagedList`, `refreshSession`).
- Відповіді містять заголовок `api-supported-versions`.
- У межах версії дозволені лише зворотно сумісні зміни: нові ендпоінти, нові необов'язкові поля запиту, нові поля відповіді. Перейменування чи видалення полів, зміна їхнього змісту або типів, нові обов'язкові поля — лише в новій версії (`v2`), яку додають поряд зі старою.
- Застарілу версію позначають `[ApiVersion("1.0", Deprecated = true)]` (клієнти бачать це в `api-deprecated-versions`) і підтримують щонайменше один реліз фронтенду, після чого видаляють.
- Health checks (`/health/*`) не версіонуються.

## Розгортання й міграції

Міграції **не** застосовуються під час старту застосунку. CI (`.github/workflows/backend-ci.yml`) збирає самодостатній EF migration bundle (`efbundle`, linux-x64) і публікує його як артефакт. Крок розгортання:

1. Зупинити трафік на нову версію (або розгортати після міграції, якщо зміни схеми зворотно сумісні).
2. Поруч із `efbundle` покласти `appsettings.json` застосунку (bundle запускає host застосунку, щоб створити `DbContext`) і задати змінні середовища, які читаються до побудови host-а: `ObservabilitySettings__ServiceName`.
3. Застосувати міграції:
   ```bash
   ./efbundle --connection "<рядок підключення до production БД>"
   ```
4. Розгорнути нову версію API. Перевірити `GET /health/ready` — він повертає `503`, доки є незастосовані міграції або БД недоступна.

Образ `migrations` із `Dockerfile` (`docker build --target migrations .`) містить bundle і `appsettings.json` і виконує саме цей крок.

## Перевірка стану

- `GET /health/live` — процес працює (без звернення до БД).
- `GET /health/ready` — БД доступна й немає незастосованих міграцій.

Обидва ендпоінти анонімні й не потрапляють під обмеження частоти запитів.

## Журнали й трасування

- Serilog пише JSON (compact) у stdout, включно з журналом HTTP-запитів; кожен запис містить `@tr` (trace id) і `@sp` (span id).
- OpenTelemetry (трасування й метрики ASP.NET Core, EF Core, HttpClient) вмикається, лише коли задано `ObservabilitySettings:OtlpEndpoint`; експорт через OTLP.
- BFF фронтенду передає заголовок `traceparent`, тож запис у журналі фронтенду й API пов'язані тим самим trace id.

## Конфігурація

| Ключ | Обов'язково в production | Опис |
| --- | --- | --- |
| `ConnectionStrings:connectionString` | так | SQL Server |
| `JwtSettings:Secret` | так | ключ підпису access-токенів, ≥ 32 символи |
| `JwtSettings:Issuer`, `JwtSettings:Audience` | так | |
| `JwtSettings:AccessTokenExpiryInMinutes` | так (15 за замовчуванням в `appsettings.json`) | |
| `RefreshTokenSettings:ExpiryInDays` | так (7) | |
| `RefreshTokenSettings:RotatedTokenReuseGracePeriodInSeconds` | так (30) | вікно, у якому щойно ротований токен можна повторно обміняти без відкликання сімейства |
| `RateLimitingSettings:*` | так (див. `appsettings.json`) | ліміти для облікових ендпоінтів за IP і загальний ліміт за користувачем |
| `CorsSettings:AllowedOrigins:N` | так | дозволені джерела (у Development — `http://localhost:3000`) |
| `ForwardedHeadersSettings:KnownProxyAddresses:N` | так, якщо API за BFF/проксі | IP-адреси довірених проксі для `X-Forwarded-For` |
| `ExternalIdentitySettings:Authority`, `ClientId`, `GroupRoleMappings:<groupId>` | лише для SSO | порожні — SSO вимкнено; часткове налаштування зупиняє старт |
| `AuditLogRetentionSettings:RetentionPeriodInDays` | так (365) | старіші записи журналу дій видаляє фонове завдання |
| `ObservabilitySettings:ServiceName` | так | ім'я сервісу в журналах і трасах |
| `ObservabilitySettings:OtlpEndpoint` | ні | порожнє — OpenTelemetry вимкнено |
| `SeedAdmin:Password` | лише для порожньої БД | пароль адміністратора за замовчуванням |
| `DemoSeed:DispatcherPassword` | ні (лише для `seed-demo` у Development) | пароль демонстраційних диспетчерів |

Усі налаштування перевіряються під час старту: відсутнє або некоректне значення зупиняє застосунок із назвою ключа й значенням у повідомленні.

## Тести

```bash
dotnet test
```

- `business_logic_tests` — швидкі тести бізнес-логіки на SQLite in-memory.
- `api_tests` — інтеграційні тести через `WebApplicationFactory` (обмеження частоти, health checks, перевірка конфігурації).
- `sql_server_integration_tests` — тести на справжньому SQL Server (паралельні призначення, глобальний фільтр депо, міграції). Запускають SQL Server у Docker через Testcontainers; без Docker тести пропускаються з поясненням. Замість Docker можна задати `SQL_SERVER_INTEGRATION_TESTS_CONNECTION_STRING` (наприклад, `Server=(localdb)\mssqllocaldb;Trusted_Connection=True`) — тести створять і видалять окрему тестову БД.
- `load-tests/conflicting-route-creation.js` — сценарій k6: N одночасних запитів на той самий інтервал авто й водія, пороги вимагають рівно одного створеного рейсу (запуск — у [docs/demo-script.md](docs/demo-script.md#результат-дослідження-одночасні-запити)).

## Документація

- [docs/01-client-and-roles.md](docs/01-client-and-roles.md) — клієнтська частина, ролі та права доступу
- [docs/02-procedures.md](docs/02-procedures.md) — процедури й алгоритми бекенду
- [docs/03-test-cases.md](docs/03-test-cases.md) — тест-кейси й звіт про дефекти
- [docs/demo-script.md](docs/demo-script.md) — 5-хвилинний сценарій демонстрації
