# Вантажі, замовлення та клієнти

Статус: проєкт, не реалізовано. Розмір: **L**.

## Проблема

Маршрут зараз — це «авто + водій + звідки/куди + час», без вантажу й без замовника. Тому:
- поле `Auto.Capacity` ніде не перевіряється, і на рейс можна «покласти» скільки завгодно;
- не видно, для якого клієнта й за яким замовленням виконано рейс;
- неможливо порахувати завантаженість авто й виручку.

## Сутності та зв'язки

| Сутність | Поля | Примітки |
|---|---|---|
| `Customer` | `Id`, `Name`, `TaxId` (ЄДРПОУ / NIP), `ContactName`, `ContactEmail`, `ContactPhone`, `BillingAddress`, `IsArchived`, `ArchivedAtUtc`, `RowVersion` | Глобальний чи прив'язаний до депо — відкрите питання. Контакти — персональні дані. |
| `Order` | `Id`, `OrderNumber` (унікальний), `CustomerId`, `Status` (`Draft`, `Confirmed`, `Scheduled`, `InTransit`, `Delivered`, `Cancelled`), `PickupLocationId`, `DeliveryLocationId`, `PickupWindowStartUtc`, `PickupWindowEndUtc`, `DeliveryWindowStartUtc`, `DeliveryWindowEndUtc`, `DepotId`, `RowVersion` | Вікна часу зберігаються в UTC і показуються в часовому поясі відповідної локації (як час маршрутів). |
| `OrderItem` | `Id`, `OrderId`, `Description`, `WeightKg` (decimal 10,2), `VolumeCubicMetres` (decimal?), `PackageCount`, `IsHazardous`, `AdrClass` (string?) | |
| `RouteOrder` | `RouteId`, `OrderId`, `SequenceNumber` | Рейс везе кілька замовлень. В першій версії замовлення не ділиться між рейсами (унікальний `OrderId`). |
| `Auto` (змінюється) | `Capacity` → `CapacityKg` (decimal 10,2) | Зараз `double` без явної одиниці; в інтерфейсі підписано «кг». |

Правило місткості: сума `OrderItem.WeightKg` усіх замовлень рейсу ≤ `Auto.CapacityKg`. Перевіряється в `RouteService`:
- під час прив'язки замовлень до рейсу;
- під час зміни авто в рейсі;
- під час зміни ваги позицій замовлення, вже прив'язаного до активного рейсу.

Прив'язка змінює `RowVersion` рейсу, тому двоє диспетчерів, які одночасно додають вантаж на один рейс, отримають `409` (механізм фази 3), а не сумарне перевантаження.

## Ендпоінти

| Метод і маршрут | Модель запиту | Модель відповіді |
|---|---|---|
| `POST /api/v1/Customer/createCustomer` | `CreateCustomerRequestModel` | `CreateCustomerResponseModel` |
| `PUT /api/v1/Customer/updateCustomer` | `UpdateCustomerRequestModel` | `204` / `409` |
| `POST /api/v1/Customer/archiveCustomer/{id}`, `POST .../restoreCustomer/{id}` | — | `204` |
| `GET /api/v1/Customer/getCustomersPagedList` | `GetCustomersPagedListRequestModel` | `GetCustomersPagedListResponseModel` |
| `GET /api/v1/Customer/getCustomerLookupOptions` | — | `GetCustomerLookupOptionsResponseModel` |
| `POST /api/v1/Order/createOrder` | `CreateOrderRequestModel` (з позиціями) | `CreateOrderResponseModel` |
| `PUT /api/v1/Order/updateOrder` | `UpdateOrderRequestModel` | `204` / `409` |
| `POST /api/v1/Order/cancelOrder/{id}` | — | `204` |
| `GET /api/v1/Order/getOrdersPagedList` | `GetOrdersPagedListRequestModel` (клієнт, статус, період, депо) | `GetOrdersPagedListResponseModel` |
| `GET /api/v1/Order/getOrderDetails/{id}` | — | `GetOrderDetailsResponseModel` |
| `PUT /api/v1/Route/assignOrdersToRoute` | `AssignOrdersToRouteRequestModel` (`RouteId`, `RouteRowVersion`, `OrderIds`) | `AssignOrdersToRouteResponseModel` (`RouteRowVersion`, `LoadedWeightKg`, `RemainingCapacityKg`) |
| `PUT /api/v1/Route/unassignOrderFromRoute` | `UnassignOrderFromRouteRequestModel` (`RouteId`, `RouteRowVersion`, `OrderId`) | `UnassignOrderFromRouteResponseModel` |

Перевищення місткості — `400` з повідомленням, що містить номер авто, місткість і сумарну вагу (за правилом явних повідомлень).

## Міграції та ризики даних

- Нові таблиці `Customers`, `Orders`, `OrderItems`, `RouteOrders`.
- **Ризик:** перейменування й зміна типу `Autos.Capacity` (`float` → `decimal(10,2)`). Перед міграцією треба перевірити реальні значення: якщо частину записано в тоннах (наприклад, `20` замість `20000`), примусова перевірка місткості почне блокувати рейси. Міграція має містити звіт-запит «підозріло малі значення» і не змінювати дані автоматично.
- Існуючі рейси не мають замовлень, тому перевірка місткості для них не спрацьовує.
- Статуси замовлення синхронізуються зі статусом рейсу (`InProgress` → `InTransit`, `Completed` → `Delivered`) у тому ж сервісі й транзакції.
- Маршрут має одну початкову й одну кінцеву локацію. Кілька точок завантаження й доставки потребують окремої моделі зупинок (`RouteStop`); це окремий великий крок і тут не проєктується.

## Екрани

- Сторінка «Клієнти» (таблиця, пошук, архів) і «Замовлення» (таблиця з фільтрами, модальне вікно з позиціями).
- Модальне вікно маршруту: секція «Вантаж» зі списком прив'язаних замовлень, кнопкою «Додати замовлення» і смугою завантаження (вага / місткість).
- Картка маршруту: відсоток завантаження.
- Звіт: завантаженість авто за період.

## Відкриті питання

1. Клієнти спільні для всіх депо чи свої в кожному?
2. Чи можна ділити одне замовлення між кількома рейсами (часткове завантаження)?
3. Чи обмежувати також об'єм і кількість палет, а не лише вагу?
4. Чи потрібні ціни, рахунки й виручка в цій системі, чи це відповідальність ERP ([08-integrations.md](08-integrations.md))?
5. Чи потрібні рейси з кількома зупинками (збірні вантажі)?
6. Небезпечні вантажі (ADR): лише позначка чи перевірка допуску водія й авто ([03-documents-and-expiry-reminders.md](03-documents-and-expiry-reminders.md))?
