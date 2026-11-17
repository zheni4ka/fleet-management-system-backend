# Пробіг (одометр) і ТО за пробігом

Статус: проєкт, не реалізовано. Розмір: **M**.

## Проблема

Зараз `AutoMaintenance` — це лише запис «авто на ТО з дати X», а статус авто перемикається вручну. Система не знає пробігу, тому не може:
- нагадати про ТО за інтервалом (наприклад, заміна мастила кожні 15 000 км або раз на рік — що настане раніше);
- рахувати вартість кілометра (див. [02-fuel-and-costs.md](02-fuel-and-costs.md));
- перевірити правдоподібність показів одометра під час заправок.

## Сутності та зв'язки

| Сутність | Поля | Примітки |
|---|---|---|
| `OdometerReading` | `Id`, `AutoId`, `OdometerKm` (int), `RecordedAtUtc`, `Source` (`OdometerReadingSource`: `Manual`, `RouteCompletion`, `Refuelling`, `Telematics`), `RecordedByUserId`, `DepotId`, `RowVersion` | Історія показів. `DepotId` копіюється з авто під час запису, щоб працював глобальний фільтр депо (як у `Route`). |
| `MaintenancePlan` | `Id`, `AutoId`, `Name`, `IntervalKm` (int?), `IntervalDays` (int?), `WarningKm`, `WarningDays`, `LastPerformedOdometerKm`, `LastPerformedOn`, `DepotId`, `RowVersion` | Регламент ТО для конкретного авто. Хоча б один з інтервалів обов'язковий. |
| `AutoMaintenance` (змінюється) | + `OdometerKm` (int?), + `MaintenancePlanId` (int?) | Під час завершення ТО за планом оновлюються `LastPerformed*` у плані. |
| `Auto` (змінюється) | + `CurrentOdometerKm` (int?) | Денормалізоване значення останнього показу для сортування й фільтрів у списку. Оновлюється в тій самій транзакції, що й `OdometerReading`. |

Зв'язки: `Auto 1—N OdometerReading`, `Auto 1—N MaintenancePlan`, `MaintenancePlan 1—N AutoMaintenance`.

Правило «наступне ТО»: `dueKm = LastPerformedOdometerKm + IntervalKm`, `dueDate = LastPerformedOn + IntervalDays`. План стає «скоро» при `CurrentOdometerKm >= dueKm - WarningKm` або `сьогодні (Europe/Kyiv) >= dueDate - WarningDays`, і «прострочено» після `dueKm`/`dueDate`. Розрахунок робить сервіс у бізнес-часовому поясі, так само як `AutoStatusSyncService`.

## Ендпоінти

| Метод і маршрут | Модель запиту | Модель відповіді |
|---|---|---|
| `POST /api/v1/OdometerReading/recordOdometerReading` | `RecordOdometerReadingRequestModel` (`AutoId`, `OdometerKm`, `RecordedAtUtc`) | `204 No Content` |
| `GET /api/v1/OdometerReading/getOdometerReadingsPagedList` | `GetOdometerReadingsPagedListRequestModel` (`AutoId`, сторінка, сортування) | `GetOdometerReadingsPagedListResponseModel` |
| `POST /api/v1/MaintenancePlan/createMaintenancePlan` | `CreateMaintenancePlanRequestModel` | `CreateMaintenancePlanResponseModel` (`Id`, `RowVersion`) |
| `PUT /api/v1/MaintenancePlan/updateMaintenancePlan` | `UpdateMaintenancePlanRequestModel` (з `RowVersion`) | `204` або `409` при конфлікті версій |
| `DELETE /api/v1/MaintenancePlan/deleteMaintenancePlan/{id}` | — | `204` |
| `GET /api/v1/MaintenancePlan/getMaintenancePlansByAutoId/{autoId}` | — | `GetMaintenancePlansByAutoIdResponseModel` (плани з розрахованими `DueKm`, `DueDate`, `DueState`) |
| `GET /api/v1/MaintenancePlan/getMaintenanceDueList` | `GetMaintenanceDueListRequestModel` (`DueState`, депо) | `GetMaintenanceDueListResponseModel` |
| `PUT /api/v1/Route/updateRoute` (змінюється) | `UpdateRouteRequestModel` + `EndOdometerKm` (int?) | без змін |

Валідація (FluentValidation): `OdometerKm` не менший за попередній показ цього авто, крім явного `IsOdometerReplaced` (заміна панелі приладів); не більший за попередній показ плюс розумну межу (наприклад, 2 000 км на добу між показами). Межу треба задати в налаштуваннях і перевіряти під час запису.

## Міграції та ризики даних

- Нові таблиці `OdometerReadings`, `MaintenancePlans`. Нові nullable-колонки `Autos.CurrentOdometerKm`, `AutoMaintenances.OdometerKm`, `AutoMaintenances.MaintenancePlanId`. Існуючі рядки не змінюються.
- В існуючих авто немає пробігу, тому до першого показу план показуватиме лише дату. У списку буде фільтр «немає показу пробігу», щоб заповнити дані.
- Індекс `(AutoId, RecordedAtUtc DESC)` для швидкого «останнього показу».
- Ризик розбіжності `Auto.CurrentOdometerKm` з історією: оновлювати лише через сервіс в одній транзакції; інтеграційний тест на SQL Server.
- Одиниці: лише кілометри. Мілі — відкрите питання.

## Екрани

- Таблиця «Автомобілі»: колонка «Пробіг», бейдж «ТО скоро / прострочено».
- Модальне вікно ТО: вкладка «Регламент» (плани з прогресом до наступного ТО), поле «Пробіг» під час реєстрації й завершення ТО.
- Картка маршруту: під час «Завершити» — діалог з полем «Пробіг на прибутті».
- Головна сторінка: блок «Найближче ТО».

## Відкриті питання

1. Хто вводить покази: диспетчер, водій (потрібна роль Driver) чи телематика?
2. Чи обов'язковий пробіг під час завершення маршруту?
3. Регламенти задаються на кожне авто чи шаблоном на модель (з копіюванням на авто)?
4. Чи потрібні мотогодини (спецтехніка, рефрижератори)?
5. Яку межу відхилення показів вважати помилкою, а яку — попередженням?
