# Паливо та витрати

Статус: проєкт, не реалізовано. Розмір: **L**.

## Проблема

Немає обліку заправок, паливних карток і витрат, тому не можна відповісти на питання «скільки коштує кілометр цього авто» і «скільки коштував цей рейс». Дані про заправки зазвичай приходять з виписок паливних карток (файли від постачальника), а не вводяться вручну.

## Сутності та зв'язки

| Сутність | Поля | Примітки |
|---|---|---|
| `FuelCard` | `Id`, `CardNumberLastDigits` (4 символи), `Provider`, `ExpiresOn`, `AssignedAutoId` (int?), `AssignedDriverId` (int?), `IsBlocked`, `DepotId`, `RowVersion` | Повний номер картки **не зберігаємо** (PCI DSS). |
| `Refuelling` | `Id`, `AutoId`, `DriverId` (int?), `RouteId` (int?), `FuelCardId` (int?), `RefuelledAtUtc`, `FuelType` (enum), `VolumeLitres` (decimal 10,3), `TotalCost` (decimal 18,2), `CurrencyCode` (ISO 4217), `OdometerKm` (int?), `StationName`, `Source` (`Manual`, `CardImport`), `ExternalTransactionId` (unique разом із `Provider`), `DepotId`, `RowVersion` | `DepotId` копіюється з авто для глобального фільтра депо. |
| `CostEntry` | `Id`, `AutoId` (int?), `RouteId` (int?), `CostCategory` (`Fuel`, `Toll`, `Parking`, `Repair`, `Insurance`, `Fine`, `Other` — нові значення лише в кінець), `Amount` (decimal 18,2), `CurrencyCode`, `IncurredOn`, `Description`, `RefuellingId` (int?), `AutoMaintenanceId` (int?), `DepotId` | Єдина таблиця для звітів. Заправка створює `CostEntry` з `RefuellingId`. |
| `ExchangeRate` | `CurrencyCode`, `RateDate`, `RateToBaseCurrency` (decimal 18,6) | Курс НБУ на дату. Базова валюта — UAH (відкрите питання). |
| `AutoMaintenance` (змінюється) | — | Вартість ремонту записується як `CostEntry` з `AutoMaintenanceId`. |

Розрахунки:
- **Вартість км** за період = сума `CostEntry` авто в базовій валюті / пробіг за період (з [01-mileage-and-maintenance.md](01-mileage-and-maintenance.md)).
- **Вартість рейсу** = прямі витрати з `RouteId` + витрати авто без прив'язки до рейсу, розподілені пропорційно пробігу рейсу (метод розподілу — відкрите питання).
- **Витрата палива** л/100 км між двома повними заправками.

## Ендпоінти

| Метод і маршрут | Модель запиту | Модель відповіді |
|---|---|---|
| `POST /api/v1/Refuelling/recordRefuelling` | `RecordRefuellingRequestModel` | `RecordRefuellingResponseModel` (`Id`, `RowVersion`) |
| `PUT /api/v1/Refuelling/updateRefuelling` | `UpdateRefuellingRequestModel` (з `RowVersion`) | `204` / `409` |
| `DELETE /api/v1/Refuelling/deleteRefuelling/{id}` | — | `204` |
| `GET /api/v1/Refuelling/getRefuellingsPagedList` | `GetRefuellingsPagedListRequestModel` | `GetRefuellingsPagedListResponseModel` |
| `POST /api/v1/Refuelling/importFuelCardTransactions` | `ImportFuelCardTransactionsRequestModel` (файл + `Provider`) | `ImportFuelCardTransactionsResponseModel` (`ImportedCount`, `DuplicateCount`, `RejectedRows`) |
| `POST /api/v1/FuelCard/createFuelCard`, `PUT .../updateFuelCard`, `POST .../blockFuelCard/{id}` | `CreateFuelCardRequestModel`, `UpdateFuelCardRequestModel` | `CreateFuelCardResponseModel` / `204` |
| `GET /api/v1/FuelCard/getFuelCardsPagedList` | `GetFuelCardsPagedListRequestModel` | `GetFuelCardsPagedListResponseModel` |
| `POST /api/v1/Cost/createCostEntry`, `PUT .../updateCostEntry`, `DELETE .../deleteCostEntry/{id}` | `CreateCostEntryRequestModel`, `UpdateCostEntryRequestModel` | `CreateCostEntryResponseModel` / `204` |
| `GET /api/v1/Cost/getCostPerKmReport` | `GetCostPerKmReportRequestModel` (період, депо, авто) | `GetCostPerKmReportResponseModel` |
| `GET /api/v1/Cost/getRouteCostReport` | `GetRouteCostReportRequestModel` (період) | `GetRouteCostReportResponseModel` |

## Міграції та ризики даних

- Лише нові таблиці; існуючі дані не змінюються.
- Гроші — `decimal(18,2)`, ніколи `double`. Звіти зберігають і оригінальну валюту, і суму в базовій валюті на дату витрати (курс фіксується під час запису, щоб звіти за минулі періоди не змінювалися).
- Імпорт: дублікати за `(Provider, ExternalTransactionId)`; незнайдене авто чи картка дає відхилений рядок зі звітом, без часткового запису рядка. Обмеження розміру файлу.
- Часові пояси: час транзакції у файлі постачальника — локальний час АЗС. Його треба конвертувати в UTC так само, як час маршрутів (часовий пояс країни АЗС або постачальника).
- Права: витрати бачить адміністратор і диспетчери свого депо (глобальний фільтр депо).

## Екрани

- Сторінка «Паливо»: таблиця заправок з фільтрами (авто, водій, період, картка), кнопки «Додати» та «Імпорт виписки» (завантаження → попередній перегляд помилок → підтвердження).
- Сторінка «Паливні картки» (лише адміністратор).
- Модальне вікно авто: вкладка «Витрати».
- Звіти: «Вартість км» і «Вартість рейсів» з експортом (див. [08-integrations.md](08-integrations.md)).

## Відкриті питання

1. Які постачальники карток (ОККО, WOG, UPG, Shell…) і в якому форматі вони дають виписки? Чи є API?
2. Базова валюта звітів — UAH? Чи потрібен облік ПДВ окремо?
3. Як розподіляти непрямі витрати (страхування, ремонт) між рейсами: за пробігом, за часом чи не розподіляти?
4. Чи потрібне погодження витрат (диспетчер вносить → адміністратор підтверджує)?
5. Чи вважати аномалією заправку, більшу за об'єм бака (потрібне нове поле `Auto.FuelTankCapacityLitres`)?
