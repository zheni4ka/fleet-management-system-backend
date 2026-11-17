# Сповіщення (email, SMS, push)

Статус: проєкт, не реалізовано. Розмір: **M** (in-app + email), **L** (+ SMS і push).

## Проблема

Події, які потребують реакції, видно лише на відповідних сторінках: ТО скоро, документ спливає, рейс змінено чи скасовано, водій перевищує робочий час. Немає каналу, щоб повідомити людину, яка зараз не дивиться в систему. Кілька модулів з інших документів спираються на цей:
- [01-mileage-and-maintenance.md](01-mileage-and-maintenance.md);
- [03-documents-and-expiry-reminders.md](03-documents-and-expiry-reminders.md);
- [04-driver-working-time.md](04-driver-working-time.md).

## Сутності та зв'язки

| Сутність | Поля | Примітки |
|---|---|---|
| `Notification` | `Id`, `RecipientUserId`, `NotificationType` (enum, нові значення лише в кінець), `PayloadJson`, `CreatedAtUtc`, `ReadAtUtc` (DateTime?) | Внутрішній центр сповіщень. `PayloadJson` — параметри шаблону, не готовий текст. |
| `NotificationDelivery` | `Id`, `NotificationId`, `NotificationChannel` (`Email`, `Sms`, `WebPush`), `Status` (`Pending`, `Sent`, `Failed`, `Skipped`), `AttemptCount`, `NextAttemptAtUtc`, `LastError`, `SentAtUtc` | Outbox: рядок створюється в тій самій транзакції, що й бізнес-подія. |
| `NotificationPreference` | `UserId`, `NotificationType`, `NotificationChannel`, `IsDisabled` | Відсутність рядка означає «увімкнено». Назва булевого поля за правилом — `IsDisabled` (за замовчуванням false). |
| `PushSubscription` | `Id`, `UserId`, `Endpoint`, `P256dhKey`, `AuthKey`, `CreatedAtUtc` | Web Push (VAPID) для браузера чи PWA. |
| `Driver` (змінюється) | + `PhoneNumber` (string?) | Водії не мають облікових записів, тому SMS водію йде на номер у картці водія. |

Відправлення: фоновий `NotificationDeliveryWorker` вибирає `Pending` з `NextAttemptAtUtc <= now`, відправляє через провайдера, повторює з експоненційною затримкою і після N спроб ставить `Failed`. Ідемпотентність — ключ `NotificationDelivery.Id` у заголовку провайдера, де це підтримується.

Мова: текст рендериться шаблоном у мові отримувача (`preferred_language` з фази 8; українська — запасна). Ключі шаблонів стабільні, як ключі повідомлень API.

Тихі години: SMS і push не надсилаються вночі в часовому поясі отримувача; часовий пояс користувача — нове поле (за замовчуванням Europe/Kyiv).

## Ендпоінти

| Метод і маршрут | Модель запиту | Модель відповіді |
|---|---|---|
| `GET /api/v1/Notification/getNotificationsPagedList` | `GetNotificationsPagedListRequestModel` (лише непрочитані, сторінка) | `GetNotificationsPagedListResponseModel` (з `UnreadCount`) |
| `PUT /api/v1/Notification/markNotificationsRead` | `MarkNotificationsReadRequestModel` (`NotificationIds`) | `204` |
| `GET /api/v1/Notification/getNotificationPreferences` | — | `GetNotificationPreferencesResponseModel` |
| `PUT /api/v1/Notification/updateNotificationPreferences` | `UpdateNotificationPreferencesRequestModel` | `204` |
| `POST /api/v1/Notification/registerPushSubscription` | `RegisterPushSubscriptionRequestModel` | `204` |
| `DELETE /api/v1/Notification/deletePushSubscription/{id}` | — | `204` |
| `GET /api/v1/Notification/getNotificationDeliveriesPagedList` (Admin) | `GetNotificationDeliveriesPagedListRequestModel` | `GetNotificationDeliveriesPagedListResponseModel` |

## Міграції та ризики даних

- Нові таблиці; nullable `Drivers.PhoneNumber` і часовий пояс користувача.
- Секрети провайдерів (SMTP або SendGrid, TurboSMS чи Twilio, VAPID-ключі) — лише в конфігурації чи сховищі секретів, обов'язкові в production, перевіряються під час старту (fail fast).
- Email: домен відправника з SPF, DKIM і DMARC, інакше листи потраплять у спам.
- SMS коштують грошей: потрібні ліміти на добу й журнал доставок для звірки з рахунком.
- Згода: для SMS водіям потрібна згода на обробку номера телефону.
- Очищення: `Notification` старші за N днів видаляються фоновим сервісом (як журнал дій із фази 5).

## Екрани

- Заголовок: дзвіночок з лічильником непрочитаних і випадним списком.
- Сторінка «Сповіщення».
- «Налаштування профілю» (нова сторінка): матриця «тип події × канал», мова, часовий пояс, підписка на push.
- Адмін: журнал доставок з причиною помилки й повторною відправкою.

## Відкриті питання

1. Які канали потрібні в першій версії? Рекомендація: in-app + email.
2. Чи будуть у водіїв облікові записи (роль Driver), чи лише SMS на номер?
3. Який SMS-провайдер і хто сплачує?
4. Хто отримує кожен тип подій: адміністратор, усі диспетчери депо чи відповідальний диспетчер рейсу (такого поля зараз немає)?
5. Чи потрібні щоденні дайджести замість окремих листів?
