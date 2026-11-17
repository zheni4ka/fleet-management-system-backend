# Документи з терміном дії та нагадування

Статус: проєкт, не реалізовано. Розмір: **M**.

## Проблема

Страховка (ОСЦПВ, «Зелена картка» для рейсів у Польщу), техогляд, посвідчення водія та інші документи мають термін дії. Зараз система зберігає лише номер посвідчення водія без дати, тому можна призначити на рейс авто з простроченою страховкою чи водія з простроченим посвідченням, і ніхто не дізнається про це заздалегідь.

## Сутності та зв'язки

| Сутність | Поля | Примітки |
|---|---|---|
| `Document` | `Id`, `DocumentType`, `AutoId` (int?), `DriverId` (int?), `Number`, `IssuedOn` (date?), `ExpiresOn` (date?), `IssuerName`, `FileStorageKey` (string?), `DepotId`, `IsArchived`, `ArchivedAtUtc`, `RowVersion` | Check-обмеження: заповнено рівно одне з `AutoId` / `DriverId`. `DepotId` береться з власника. |
| `DocumentType` (enum) | `MandatoryInsurance`, `GreenCard`, `TechnicalInspection`, `VehicleRegistration`, `DriverLicence`, `MedicalCertificate`, `TachographCard`, `AdrCertificate` | Нові значення лише в кінець. Тип визначає, кому він належить (авто чи водій). |
| `DocumentRequirement` | `Id`, `DocumentType`, `IsRouteAssignmentBlocked`, `CountryCode` (string?) | Які документи обов'язкові для призначення на рейс (наприклад, `GreenCard` — лише для рейсів за межі України). |
| `DocumentExpiryReminder` | `Id`, `DocumentId`, `ReminderThresholdDays`, `SentAtUtc` | Журнал надісланих нагадувань. Унікальний `(DocumentId, ReminderThresholdDays)`, щоб не надсилати двічі. |

Пороги нагадувань (наприклад, 30, 14, 3 дні) задаються в налаштуваннях і перевіряються під час запису налаштувань. Дати закінчення — календарні дати; «сьогодні» рахується в бізнес-часовому поясі (Europe/Kyiv).

Перевірка під час призначення рейсу (`RouteService`): для авто й водія на дату відправлення в часовому поясі початкової локації шукаються документи з `DocumentRequirement.IsRouteAssignmentBlocked`, у яких `ExpiresOn` раніше за дату прибуття. Залежно від рішення це блокування (`400` з переліком документів) або попередження у відповіді.

## Ендпоінти

| Метод і маршрут | Модель запиту | Модель відповіді |
|---|---|---|
| `POST /api/v1/Document/uploadDocumentFile` | `UploadDocumentFileRequestModel` (multipart) | `UploadDocumentFileResponseModel` (`FileStorageKey`) |
| `POST /api/v1/Document/createDocument` | `CreateDocumentRequestModel` | `CreateDocumentResponseModel` (`Id`, `RowVersion`) |
| `PUT /api/v1/Document/updateDocument` | `UpdateDocumentRequestModel` (з `RowVersion`) | `204` / `409` |
| `POST /api/v1/Document/archiveDocument/{id}` | — | `204` |
| `GET /api/v1/Document/getDocumentsPagedList` | `GetDocumentsPagedListRequestModel` (власник, тип, стан) | `GetDocumentsPagedListResponseModel` |
| `GET /api/v1/Document/getExpiringDocumentsList` | `GetExpiringDocumentsListRequestModel` (`DaysAhead`, депо) | `GetExpiringDocumentsListResponseModel` |
| `GET /api/v1/Document/downloadDocumentFile/{id}` | — | файл (лише з перевіркою депо) |
| `GET /api/v1/Route/getRouteAssignmentDocumentIssues` | `GetRouteAssignmentDocumentIssuesRequestModel` (`AutoId`, `DriverId`, час, локації) | `GetRouteAssignmentDocumentIssuesResponseModel` |

## Міграції та ризики даних

- Нова таблиця `Documents` і довідник `DocumentRequirements`. `Driver.LicenseNumber` залишається: міграція створює для кожного водія документ `DriverLicence` з цим номером і **порожньою** `ExpiresOn`.
- Ризик: у всіх існуючих документах немає дат, тож перевірка ігнорує документи без `ExpiresOn` і показує список «Не заповнено дату». Вмикати блокування можна лише після заповнення.
- Скани документів — персональні дані. Потрібне сховище файлів (Azure Blob або диск) поза БД, обмеження розміру й типу (PDF, JPEG, PNG), доступ лише через API з фільтром депо, політика зберігання.
- Нагадування надсилає фоновий сервіс через модуль сповіщень ([07-notifications.md](07-notifications.md)).

## Екрани

- Модальні вікна авто та водія: вкладка «Документи» (список, додати, завантажити скан, архівувати).
- Сторінка «Документи»: «Спливають протягом N днів», «Прострочені», «Без дати».
- Головна: KPI «Документи, що спливають».
- Додавання/редагування маршруту: попередження або блокування з переліком проблемних документів поруч із вибором авто й водія.

## Відкриті питання

1. Прострочений документ блокує призначення чи лише попереджає? Чи може адміністратор зробити виняток (з записом у журнал дій)?
2. Які документи обов'язкові для внутрішніх рейсів, а які — для рейсів у Польщу та інші країни?
3. Чи потрібно зберігати скани, і як довго?
4. Кому надсилати нагадування: адміністратору, диспетчерам депо, самому водію?
5. Чи потрібні категорії посвідчення (C, CE) з перевіркою відповідності типу авто?
