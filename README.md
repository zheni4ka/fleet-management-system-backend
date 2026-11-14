# Fleet Management System — backend

REST API системи обліку автопарку й логістики маршрутів (ASP.NET Core, .NET 10, EF Core, SQL Server).

## Запуск локально

1. Застосувати міграції до LocalDB:
   ```bash
   dotnet ef database update --project data_access --startup-project transport-logistic-management-2026
   ```
2. Налаштувати секрети. Їх немає в `appsettings.json`, вони зберігаються в user-secrets:
   ```bash
   dotnet user-secrets set "JwtSettings:Secret" "<випадковий рядок, не менше 64 символів>" --project transport-logistic-management-2026
   ```
   Для нової БД, де ще немає адміністратора, потрібен також пароль адміністратора за замовчуванням (логін `admin`):
   ```bash
   dotnet user-secrets set "SeedAdmin:Password" "<пароль>" --project transport-logistic-management-2026
   ```
3. Запустити API (http://localhost:5196, документація — `/scalar/v1`):
   ```bash
   dotnet run --project transport-logistic-management-2026 --launch-profile http
   ```

Поза середовищем Development ті самі значення передаються змінними середовища `JwtSettings__Secret` і `SeedAdmin__Password`.

## Документація

- [docs/01-client-and-roles.md](docs/01-client-and-roles.md) — клієнтська частина, ролі та права доступу
- [docs/02-procedures.md](docs/02-procedures.md) — процедури й алгоритми бекенду
- [docs/03-test-cases.md](docs/03-test-cases.md) — тест-кейси й звіт про дефекти
