# Процедури обробки даних (backend)

Документ закриває вимоги лабораторних робіт № 4 (перелік процедур, потрібних для кожної категорії користувачів) і № 5 (алгоритми обробки інформації, запити для різних категорій, коди процедур).

Бекенд складається з трьох шарів:

- `transport-logistic-management-2026` — REST-контролери, JWT, глобальний обробник винятків;
- `business_logic` — сервіси, валідатори (FluentValidation), специфікації запитів (Ardalis.Specification), DTO;
- `data_access` — EF Core `DbContext`, репозиторій, Unit of Work, міграції.

Контролери лише приймають запит і викликають сервіс. Перевірка даних і бізнес-правила виконуються в сервісах. Кожна зміна даних записується в журнал дій (`IAuditLogService.LogActionAsync`) і зберігається одним `SaveChangesAsync`, тобто в одній транзакції.

## 1. Перелік процедур

### Облікові записи — `AccountService`
| Процедура | Ендпоінт | Роль | Опис |
|---|---|---|---|
| `LoginAsync` | `POST /api/Account/login` | без авторизації | Перевіряє логін (або email) і пароль, повертає JWT з ролями |
| `RegisterAsync` | `POST /api/Account/register` | Admin | Створює користувача; перевіряє унікальність логіна та email; призначає роль |
| `GetDispatchersAsync` | `GET /api/Account/all` | Admin | Список диспетчерів |
| `GetDispatcherByIdAsync` | `GET /api/Account/{id}` | Admin | Диспетчер за ID |
| `DeleteUserAsync` | `DELETE /api/Account/{id}` | Admin | Видаляє користувача (власний обліковий запис видалити не можна) |

### Автомобілі — `AutoService`
| Процедура | Ендпоінт | Роль | Опис |
|---|---|---|---|
| `GetAllAsync` / `GetAsync` | `GET /api/Auto/all`, `/{id}` | усі автентифіковані | Перегляд |
| `CreateAsync` | `POST /api/Auto` | Admin | Валідація (марка, модель, колір, вантажопідйомність > 0, формат номера); перевірка унікальності номера |
| `EditAsync` | `PUT /api/Auto` | Admin | Редагування з тими самими перевірками |
| `UpdateStatusAsync` | `PATCH /api/Auto/{id}/status` | Admin, Dispatcher | Зміна статусу: `Available`, `InService` або `UnderMaintenance` |
| `DeleteAsync` | `DELETE /api/Auto/{id}` | Admin | Забороняє видаляти авто, що є в маршрутах |

### Водії — `DriverService`
| Процедура | Ендпоінт | Роль | Опис |
|---|---|---|---|
| `GetAllAsync` / `GetAsync` | `GET /api/Driver/all`, `/{id}` | усі автентифіковані | Перегляд |
| `CreateAsync` / `EditAsync` | `POST`, `PUT /api/Driver` | Admin | ПІБ українськими літерами (допускаються апостроф і дефіс); посвідчення у форматі `ABC 123456`; посвідчення має бути унікальним; необов'язкові контакти: телефон у форматі E.164 (`+380671234567`) і електронна пошта, порожні значення зберігаються як `NULL` |
| `DeleteAsync` | `DELETE /api/Driver/{id}` | Admin | Забороняє видаляти водія з активними маршрутами; водія, на якого посилаються маршрути або журнал поїздок, архівує замість видалення |

### Журнал поїздок водіїв — `DriverTripService`
Окрема таблиця `DriverTrips` зберігає знімок кожної поїздки: місто й країну відправлення та прибуття з часовими поясами, авто, плановий і фактичний час, тривалість у хвилинах і підсумковий статус. Знімок не залежить від маршруту: якщо маршрут видалити, запис лишається з `RouteId = NULL`.

| Процедура | Ендпоінт | Роль | Опис |
|---|---|---|---|
| `RecordFinishedTripAsync` | — (викликається з `RouteService.UpdateAsync` і `AutoMaintenanceService.CreateAsync`) | — | Записує поїздку, коли маршрут, що вже почався (є фактичне відправлення), переходить у `Completed` або `Cancelled`; повторно той самий маршрут не записується |
| `GetPagedListAsync` | `GET /api/v1/Driver/getDriverTripsPagedList?driverId=…` | Admin, Dispatcher | Історія поїздок водія з пагінацією, від нових до старих; сортування за початком або тривалістю |
| `GetSummaryAsync` | `GET /api/v1/Driver/getDriverTripSummary/{driverId}` | Admin, Dispatcher | Кількість поїздок (усього, виконаних, перерваних), сумарні хвилини в дорозі й час завершення останньої поїздки |

Міграція `AddDriverContactsAndTrips` заповнює журнал з уже виконаних маршрутів (фактичний час, а якщо його немає — плановий).

### Локації — `LocationService`
| Процедура | Ендпоінт | Роль | Опис |
|---|---|---|---|
| `GetAllAsync` / `GetAsync` | `GET /api/Location/all`, `/{id}` | усі автентифіковані | Перегляд довідника |
| `SearchCityGeocodingCandidatesAsync` | `GET /api/v1/Location/searchCityGeocodingCandidates` | Admin, Dispatcher | Пошук міст через Open-Meteo Geocoding API мовою інтерфейсу |
| `SearchAddressCandidatesAsync` | `GET /api/v1/Location/searchAddressCandidates?searchText=…&latitude=…&longitude=…` | Admin, Dispatcher | Пошук вулиць і будинків через Photon (OpenStreetMap) у прямокутнику ±0,25° широти й ±0,35° довготи навколо обраного міста; дублікати однієї адреси прибираються |
| `EnsureCityLocationAsync` | `POST /api/v1/Location/ensureCityLocation` | Admin, Dispatcher | Повертає наявну локацію за GeoNames ID або створює нову з координатами й часовим поясом |
| `FillMissingCoordinatesAsync` | `POST /api/v1/Location/fillMissingLocationCoordinates` | Admin | Дозаповнює координати старих локацій і повертає ті, що не вдалося визначити |
| `DeleteAsync` | `DELETE /api/Location/{id}` | Admin | Забороняє видаляти локацію, що використовується в маршрутах |

### Маршрути — `RouteService`
| Процедура | Ендпоінт | Роль | Опис |
|---|---|---|---|
| `GetAllAsync` / `GetAsync` | `GET /api/Route/all`, `/{id}` | усі автентифіковані | Перегляд |
| `GetRoutesBySearchFilterAsync` | `GET /api/Route/search` | Admin, Dispatcher | Пошук за кількома критеріями (див. алгоритм 2.3) |
| `GetRoutesPeriodReportAsync` | `GET /api/Route/periodReport` | Admin, Dispatcher | Звіт за період (див. алгоритм 2.4) |
| `CreateAsync` | `POST /api/Route` | Admin, Dispatcher | Створення рейсу (див. алгоритм 2.1) |
| `UpdateAsync` | `PUT /api/Route/{id}` | Admin, Dispatcher | Редагування й зміна статусу (див. алгоритм 2.2) |
| `DeleteAsync` | `DELETE /api/Route/{id}` | Admin, Dispatcher | Видалення; звільняє авто, якщо рейс був активним |

### Техобслуговування — `AutoMaintenanceService`
| Процедура | Ендпоінт | Роль | Опис |
|---|---|---|---|
| `GetAllAsync` / `GetAsync` / `GetByAutoIdAsync` | `GET /api/AutoMaintenance/...` | усі автентифіковані | Перегляд, зокрема історія ТО конкретного авто |
| `CreateAsync` | `POST /api/AutoMaintenance` | Admin | Ставить авто на ТО й скасовує його рейси (див. алгоритм 2.5) |
| `UpdateAsync` | `PUT /api/AutoMaintenance` | Admin | Позначене завершеним ТО повертає авто в статус `Available` |
| `DeleteAsync` | `DELETE /api/AutoMaintenance/{id}` | Admin | Видалення незавершеного ТО звільняє авто |

### Журнал дій — `AuditLogService`
| Процедура | Ендпоінт | Роль | Опис |
|---|---|---|---|
| `LogActionAsync` | — (викликається з інших сервісів) | — | Записує дію, ID поточного користувача, час (UTC) і ID об'єкта |
| `GetLogsAsync` | `GET /api/AuditLog/all` | Admin | Журнал від нових записів до старих, з логіном користувача |

## 2. Алгоритми

### 2.1. Створення маршруту (`RouteService.CreateAsync`)
1. Перевірити модель: ID локацій, авто й водія > 0; пункт відправлення ≠ пункт прибуття; час відправлення < часу прибуття; статус — допустиме значення.
2. Знайти авто (інакше 404). Якщо авто на ТО (`UnderMaintenance`), відмовити: «Автомобіль на ремонті».
3. Перевірити, що водій і обидві локації існують (інакше 404).
4. Перевірити зайнятість. Якщо в того самого авто або водія є не скасований і не завершений рейс, інтервал якого перетинається з новим (`існуючий.Відправлення < нове.Прибуття` **і** `існуючий.Прибуття > нове.Відправлення`), відмовити. Перевірка виконується в БД (специфікації `OverlappingForAuto` і `OverlappingForDriver`).
5. Зберегти маршрут. Якщо його одразу створено зі статусом `InProgress`, перевести авто в `InService`.
6. Записати `RouteCreated` у журнал.

### 2.2. Редагування маршруту й зміна статусу (`RouteService.UpdateAsync`)
1. Перевірити модель, узяти блокування авто й водія (`sp_getapplock`) і знайти маршрут. Адреси відправлення й призначення під час редагування необов'язкові (під час створення — обов'язкові), але передана адреса має містити і рядок, і координати; непередана адреса не змінюється.
2. Перевірити перехід статусу за матрицею `RouteStatusTransitions`: `Planned → InProgress | Cancelled`, `InProgress → Completed | Cancelled`; статуси `Completed` і `Cancelled` кінцеві. Інакше — 400.
3. Для маршруту в кінцевому статусі заборонити зміну авто чи водія (400).
4. Якщо передано новий `departureTimeUtc`/`arrivalTimeUtc` (тільки разом), дозволити це лише для `Planned` (інакше 400).
5. Якщо авто замінили, нове авто не може бути на ТО. Поточне авто можна залишити, навіть якщо воно вже на ремонті: так диспетчер може скасувати його рейс.
6. Для активного цільового статусу перевірити зайнятість водія, авто й перетин з незавершеним ТО за **новим** інтервалом рейсу, не враховуючи сам маршрут (409 у разі конфлікту).
7. На вході в `InProgress` записати `ActualDepartureTimeUtc`, на вході в `Completed` — `ActualArrivalTimeUtc` (поточний час UTC).
8. Синхронізувати статуси поточного й попереднього авто й записати `RouteUpdated` у журнал.

### 2.3. Пошук маршрутів (`RouteService.GetRoutesBySearchFilterAsync`)
Вхід: `startCity`, `destinationCity`, `departureFromDate`, `departureToDate`, `status`, `driverId`, `autoId`. Усі параметри необов'язкові.
1. Перевірити: дата «по» ≥ дати «з»; статус допустимий; ID > 0.
2. Скласти специфікацію `RouteSpecs.BySearchFilter`, яка додає умову `WHERE` лише для заданих параметрів:
   - місто: `LIKE %значення%` (без урахування регістру — за налаштуванням SQL Server);
   - дата «з»: `DepartureTime >= дата 00:00`;
   - дата «по»: `DepartureTime < (дата + 1 день) 00:00`, тобто весь день включно;
   - статус, водій, авто: точний збіг.
3. Відсортувати за часом відправлення. Фільтрування виконується в SQL, тому таблиця повністю в пам'ять не завантажується.

### 2.4. Звіт за період (`RouteService.GetRoutesPeriodReportAsync`)
Вхід: `periodStartDate`, `periodEndDate` (дати без часу).
1. Перевірити: кінець ≥ початку; період не довший за 366 днів.
2. Межі періоду — початок дня `periodStartDate` і початок дня після `periodEndDate` за київським часом, переведені в UTC.
3. Вибрати з БД рейси, інтервал яких перетинається з періодом: `Відправлення < кінець періоду` і `Прибуття > початок періоду`, де для відправлення й прибуття беруться фактичні значення, якщо вони є, інакше планові (`RouteSpecs.CreateOverlappingPeriodCriteria`).
4. Обчислити для відібраних рейсів:
   - загальну кількість і кількість за кожним статусом;
   - години в дорозі для завершених рейсів: сума тривалостей, обрізаних межами періоду, округлена до 0,1 год;
   - кількість різних авто й водіїв;
   - навантаження кожного водія: кількість рейсів, кількість виконаних і години в дорозі. Водії сортуються за кількістю рейсів від більшої до меншої.
5. Повернути показники разом зі списком рейсів.

### 2.5. Постановка авто на ТО (`AutoMaintenanceService.CreateAsync`)
1. Перевірити модель (дата ТО не в минулому; необов'язковий `PlannedEndDate` пізніше за початок), узяти блокування авто й знайти авто.
2. Знайти рейси цього авто зі статусом `Planned` або `InProgress`, що перетинаються з інтервалом ТО — від `ServiceDate` до `PlannedEndDate`, а без кінця — без верхньої межі (`ActiveByAutoOverlappingMaintenanceInterval`), і перевести їх у `Cancelled`. Незавершене ТО без кінця блокує авто для нових рейсів, доки його не завершать.
3. Створити запис ТО й перевести авто в `UnderMaintenance`.
4. Записати `MaintenanceCreated` у журнал. Скасування рейсів і створення ТО зберігаються разом.

## 3. Коди ключових процедур

Нижче наведено фрагменти з `business_logic/Services/RouteService.cs` і `business_logic/Specifications/RouteSpecs.cs`.

### Перевірка зайнятості авто й водія
```csharp
private async Task EnsureResourcesAreFreeAsync(int autoId, int driverId, DateTime departure, DateTime arrival, int? excludeRouteId = null)
{
    if (await _routeRepository.AnyBySpecAsync(new RouteSpecs.OverlappingForAuto(autoId, departure, arrival, excludeRouteId)))
    {
        throw new BusinessRuleException("Автомобіль вже зайнятий на іншому рейсі у цей час.");
    }

    if (await _routeRepository.AnyBySpecAsync(new RouteSpecs.OverlappingForDriver(driverId, departure, arrival, excludeRouteId)))
    {
        throw new BusinessRuleException("Водій вже зайнятий на іншому рейсі у цей час.");
    }
}
```

```csharp
public OverlappingForAuto(int autoId, DateTime departure, DateTime arrival, int? excludeRouteId = null)
{
    Query.Where(r => r.AutoId == autoId
        && r.Status != RouteStatus.Cancelled
        && r.Status != RouteStatus.Completed
        && r.DepartureTime < arrival
        && r.ArrivalTime > departure);

    if (excludeRouteId.HasValue)
    {
        Query.Where(r => r.Id != excludeRouteId.Value);
    }
}
```

### Створення маршруту
```csharp
public async Task<RouteDTO> CreateAsync(CreateRouteModel model)
{
    await _createValidator.ValidateAndThrowAsync(model);

    var auto = await GetUsableAutoAsync(model.AutoId);
    await EnsureDriverExistsAsync(model.DriverId);
    await EnsureLocationsExistAsync(model.StartLocationId, model.DestinationLocationId);
    await EnsureResourcesAreFreeAsync(model.AutoId, model.DriverId, model.DepartureTime, model.ArrivalTime);

    var route = _mapper.Map<Route>(model);
    await _routeRepository.InsertAsync(route);

    if (route.Status == RouteStatus.InProgress)
    {
        auto.Status = AutoStatus.InService;
    }

    await _unitOfWork.SaveChangesAsync();
    await _auditLogService.LogActionAsync(AuditAction.RouteCreated, route.Id);
    await _unitOfWork.SaveChangesAsync();

    return _mapper.Map<RouteDTO>(route);
}
```

### Звіт за період
```csharp
public async Task<GetRoutesPeriodReportResponseModel> GetRoutesPeriodReportAsync(GetRoutesPeriodReportRequestModel periodReportRequest)
{
    await _periodReportValidator.ValidateAndThrowAsync(periodReportRequest);

    var periodStartDateTime = periodReportRequest.PeriodStartDate.ToDateTime(TimeOnly.MinValue);
    var periodEndExclusiveDateTime = periodReportRequest.PeriodEndDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
    var periodRoutes = (await _routeRepository.GetListBySpecAsync(
        new RouteSpecs.ByDeparturePeriod(periodStartDateTime, periodEndExclusiveDateTime))).ToList();

    var driverSummaries = periodRoutes
        .GroupBy(route => route.DriverId)
        .Select(driverRoutes => new GetRoutesPeriodReportDriverSummaryResponseModel(
            DriverId: driverRoutes.Key,
            RoutesCount: driverRoutes.Count(),
            CompletedRoutesCount: driverRoutes.Count(route => route.Status == RouteStatus.Completed),
            CompletedRoutesDrivingHours: GetCompletedRoutesDrivingHours(driverRoutes)))
        .OrderByDescending(driverSummary => driverSummary.RoutesCount)
        .ToList();

    return new GetRoutesPeriodReportResponseModel(
        PeriodStartDate: periodReportRequest.PeriodStartDate,
        PeriodEndDate: periodReportRequest.PeriodEndDate,
        TotalRoutesCount: periodRoutes.Count,
        PlannedRoutesCount: periodRoutes.Count(route => route.Status == RouteStatus.Planned),
        InProgressRoutesCount: periodRoutes.Count(route => route.Status == RouteStatus.InProgress),
        CompletedRoutesCount: periodRoutes.Count(route => route.Status == RouteStatus.Completed),
        CancelledRoutesCount: periodRoutes.Count(route => route.Status == RouteStatus.Cancelled),
        CompletedRoutesDrivingHours: GetCompletedRoutesDrivingHours(periodRoutes),
        InvolvedAutosCount: periodRoutes.Select(route => route.AutoId).Distinct().Count(),
        InvolvedDriversCount: driverSummaries.Count,
        DriverSummaries: driverSummaries,
        Routes: _mapper.Map<List<RouteDTO>>(periodRoutes));
}
```

### Постановка авто на ТО зі скасуванням рейсів
```csharp
var affectedRoutes = await _routeRepository.GetListBySpecAsync(new RouteSpecs.ActiveByAutoFrom(model.AutoId, model.ServiceDate));
foreach (var route in affectedRoutes)
{
    route.Status = RouteStatus.Cancelled;
}

var maintenance = _mapper.Map<AutoMaintenance>(model);
await _maintenanceRepository.InsertAsync(maintenance);
auto.Status = AutoStatus.UnderMaintenance;
```

## 4. Обробка помилок

`GlobalExceptionHandler` перетворює винятки на відповіді у форматі `ProblemDetails`:

| Виняток | HTTP-код | Коли виникає |
|---|---|---|
| `ValidationException` (FluentValidation) | 400 | Некоректні вхідні дані; помилки згруповано за полями |
| `BusinessRuleException` | 400 | Порушено бізнес-правило (зайнятий водій, авто на ремонті тощо) |
| `NotFoundException` | 404 | Об'єкт не знайдено |
| `UnauthorizedAccessException` | 401 | Неправильний логін або пароль |
| інші | 500 | Неочікувана помилка (записується в лог) |

Клієнт показує повідомлення з `ProblemDetails` у спливному сповіщенні (toast).
