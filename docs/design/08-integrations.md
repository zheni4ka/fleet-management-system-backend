# Інтеграції: ERP, бухгалтерія, webhooks, імпорт і експорт CSV/Excel

Статус: проєкт, не реалізовано. Розмір: **M** (webhooks + CSV/Excel), **L** (кожен ERP-конектор окремо).

## Проблема

Дані автопарку живуть лише в цій системі. Їх неможливо:
- вивантажити для бухгалтерії;
- первинно завантажити з таблиці (десятки авто й водіїв уручну);
- передати в ERP (SAP, BAS) чи інші системи в момент змін.

Без цього система дублює ручну роботу.

Рекомендований порядок:
1. Експорт CSV/Excel зі сторінок списків і імпорт довідників — найшвидша користь.
2. Загальні вихідні webhooks — дають інтеграцію з будь-чим через middleware.
3. Конектори конкретних ERP — окремим сервісом, коли буде відома цільова система.

## Сутності та зв'язки

| Сутність | Поля | Примітки |
|---|---|---|
| `ImportJob` | `Id`, `ImportKind` (`Autos`, `Drivers`, `Locations`, `FuelCardTransactions`), `FileName`, `Status` (`Pending`, `Validated`, `Imported`, `Failed`), `RowsTotal`, `RowsImported`, `RowsRejected`, `CreatedByUserId`, `CreatedAtUtc`, `DepotId` | Імпорт у два кроки: перевірка → підтвердження. |
| `ImportJobError` | `Id`, `ImportJobId`, `RowNumber`, `ColumnName`, `Message` | Помилки рядків для показу користувачу. |
| `WebhookSubscription` | `Id`, `Name`, `TargetUrl`, `EncryptedSigningSecret`, `EventTypes`, `IsDisabled`, `DepotId` (int?), `RowVersion` | Секрет шифрується (ASP.NET Data Protection); показується лише один раз під час створення. |
| `WebhookDelivery` | `Id`, `WebhookSubscriptionId`, `EventType`, `EventId` (Guid), `PayloadJson`, `Status`, `AttemptCount`, `NextAttemptAtUtc`, `ResponseStatusCode`, `LastError` | Outbox, як у [07-notifications.md](07-notifications.md). |
| `ExternalReference` | `EntityType`, `EntityId`, `ExternalSystemName`, `ExternalId` | Відповідність ID для ERP. Унікальні `(ExternalSystemName, EntityType, ExternalId)` і `(ExternalSystemName, EntityType, EntityId)`. |

Події webhooks (перша версія): `route.created`, `route.statusChanged`, `route.deleted`, `auto.statusChanged`, `maintenance.completed`, а також `refuelling.recorded` і `costEntry.created` після [02-fuel-and-costs.md](02-fuel-and-costs.md).

Формат події: `{ "eventId", "eventType", "version": 1, "occurredAtUtc", "data": {...} }`. Заголовки:
- `X-Fleet-Signature: sha256=<HMAC тіла>`;
- `X-Fleet-Timestamp`.

Отримувач відкидає повтори за `eventId`. Повторні спроби — з експоненційною затримкою; після N невдач підписка вимикається з записом у журнал дій.

CSV/Excel:
- Експорт використовує ті самі моделі фільтрів і сортування, що й сторінки списків, тож дані збігаються з екраном. Фільтр депо діє і тут.
- Excel — бібліотека ClosedXML (MIT). EPPlus має комерційну ліцензію.
- CSV — UTF-8 з BOM (щоб Excel правильно відкрив кирилицю), роздільник `;` для української локалі.
- Захист від CSV-ін'єкцій: значення, що починаються з `=`, `+`, `-`, `@`, екрануються.
- Імпорт шукає дублікати за природними ключами: `Auto.Number`, `Driver.LicenseNumber`, `(Location.City, Location.Country)`.

ERP:
- **BAS / 1С-подібні**: обмін через OData-інтерфейс бази або файлами формату EnterpriseData (XML) за розкладом.
- **SAP S/4HANA**: OData API або IDoc через інтеграційну платформу (SAP Integration Suite).

В обох випадках конектор — окремий процес, що слухає webhooks цієї системи й пише в `ExternalReference` через API.

## Ендпоінти

| Метод і маршрут | Модель запиту | Модель відповіді |
|---|---|---|
| `GET /api/v1/Auto/exportAutos` (і `exportDrivers`, `exportLocations`, `exportRoutes`, `exportAuditLogs`) | ті самі моделі фільтрів, що й `get…PagedList`, + `ExportFileFormat` (`Csv`, `Xlsx`) | файл |
| `POST /api/v1/Import/startImportJob` | `StartImportJobRequestModel` (multipart: файл, `ImportKind`) | `StartImportJobResponseModel` (`ImportJobId`, `RowsTotal`, `RowsRejected`) |
| `GET /api/v1/Import/getImportJobDetails/{id}` | — | `GetImportJobDetailsResponseModel` (з помилками рядків) |
| `POST /api/v1/Import/confirmImportJob/{id}` | — | `ConfirmImportJobResponseModel` (`RowsImported`) |
| `GET /api/v1/Import/downloadImportTemplate` | `GetImportTemplateRequestModel` (`ImportKind`, формат) | файл-шаблон |
| `POST /api/v1/Webhook/createWebhookSubscription` | `CreateWebhookSubscriptionRequestModel` | `CreateWebhookSubscriptionResponseModel` (`Id`, `SigningSecret` — лише тут) |
| `PUT /api/v1/Webhook/updateWebhookSubscription` | `UpdateWebhookSubscriptionRequestModel` | `204` / `409` |
| `DELETE /api/v1/Webhook/deleteWebhookSubscription/{id}` | — | `204` |
| `POST /api/v1/Webhook/sendTestWebhookEvent/{id}` | — | `SendTestWebhookEventResponseModel` (код відповіді отримувача) |
| `GET /api/v1/Webhook/getWebhookDeliveriesPagedList` | `GetWebhookDeliveriesPagedListRequestModel` | `GetWebhookDeliveriesPagedListResponseModel` |
| `PUT /api/v1/ExternalReference/upsertExternalReference` | `UpsertExternalReferenceRequestModel` | `204` |

Імпорт, webhooks і зовнішні посилання доступні лише адміністратору. Експорт доступний і диспетчерам, але в межах їхніх депо.

## Міграції та ризики даних

- Лише нові таблиці.
- Імпорт підтверджується однією транзакцією: або всі коректні рядки, або нічого. Обмеження файлу: 10 МБ і 50 000 рядків.
- Персональні дані в експорті (ПІБ водіїв, номери посвідчень): кожен експорт пишеться в журнал дій.
- Секрети підписів зашифровані; ключі Data Protection мають зберігатися поза контейнером (інакше після перезапуску секрети не розшифруються).
- `TargetUrl` webhooks — захист від SSRF: лише `https`, заборона приватних і loopback-адрес.
- Часові поля в експорті: UTC і окремо місцевий час локації, щоб бухгалтерія не плутала зони.

## Екрани

- Кнопка «Експорт» (CSV / Excel) на сторінках списків, з поточними фільтрами.
- Майстер імпорту: вибір типу → завантаження → таблиця помилок → «Імпортувати N рядків».
- Адмін, сторінка «Інтеграції»: підписки webhooks, журнал доставок, «Надіслати тестову подію».

## Відкриті питання

1. Яка саме ERP і версія (BAS Бухгалтерія, BAS ERP, SAP), і хто відповідає за інтеграцію з її боку?
2. Хто «головний» для довідників (водії, авто): ця система чи ERP?
3. Що потрібно бухгалтерії: витрати, пробіг, рейси? У якому форматі й як часто?
4. Чи потрібен вхідний API для зовнішніх систем (створення замовлень з ERP), і як автентифікувати машини (client credentials в Entra ID)?
5. Які поля обов'язкові в шаблонах імпорту, і що робити з невідомим депо?
