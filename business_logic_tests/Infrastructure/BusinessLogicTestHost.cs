using business_logic;
using business_logic.Constants;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic.Settings;
using data_access;
using data_access.data;
using data_access.Interceptors;
using data_access.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace business_logic_tests.Infrastructure
{
    public sealed class BusinessLogicTestHost : IDisposable
    {
        public const int StartLocationId = 1;
        public const int DestinationLocationId = 2;
        public const int TestRefreshTokenExpiryInDays = 7;
        public const int TestAuditLogRetentionPeriodInDays = 365;
        public const string AdminGroupId = "group-admins";
        public const string DispatcherGroupId = "group-dispatchers";

        private readonly SqliteConnection _sqliteConnection;
        private readonly ServiceProvider _serviceProvider;

        public BusinessLogicTestHost() : this(rotatedTokenReuseGracePeriodInSeconds: 0)
        {
        }

        public BusinessLogicTestHost(int rotatedTokenReuseGracePeriodInSeconds)
        {
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _sqliteConnection.Open();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddBusinessLogicServices();
            services.AddDbContext<FMS_DbContext>(options => options.UseSqlite(_sqliteConnection).AddInterceptors(new AuditLogChangesInterceptor()));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDatabaseApplicationLockService, TestDatabaseApplicationLockService>();
            services.AddSingleton<TestCurrentUserScope>();
            services.AddScoped<ICurrentUserService, TestCurrentUserService>();
            services.AddScoped<ITokenService, TestTokenService>();
            services.AddAuthentication();
            services.AddAccountIdentity();
            services.AddOptions<AuditLogRetentionSettings>().Configure(auditLogRetentionSettings =>
            {
                auditLogRetentionSettings.RetentionPeriodInDays = BusinessLogicTestHost.TestAuditLogRetentionPeriodInDays;
            });
            services.AddOptions<RefreshTokenSettings>().Configure(refreshTokenSettings =>
            {
                refreshTokenSettings.ExpiryInDays = TestRefreshTokenExpiryInDays;
                refreshTokenSettings.RotatedTokenReuseGracePeriodInSeconds = rotatedTokenReuseGracePeriodInSeconds;
            });
            services.AddOptions<ExternalIdentitySettings>().Configure(externalIdentitySettings =>
            {
                externalIdentitySettings.Authority = "https://login.microsoftonline.com/test-tenant/v2.0";
                externalIdentitySettings.ClientId = "test-client";
                externalIdentitySettings.GroupRoleMappings = new Dictionary<string, string>
                {
                    [AdminGroupId] = UserRoles.Admin,
                    [DispatcherGroupId] = UserRoles.Dispatcher
                };
            });
            services.AddSingleton<TestExternalIdentityTokenValidator>();
            services.AddSingleton<IExternalIdentityTokenValidator>(serviceProvider => serviceProvider.GetRequiredService<TestExternalIdentityTokenValidator>());
            services.AddOptions<CityGeocodingSettings>().Configure(cityGeocodingSettings =>
            {
                cityGeocodingSettings.BaseUrl = "https://geocoding.test/v1/";
                cityGeocodingSettings.StoredNamesLanguageCode = SupportedLanguages.English;
                cityGeocodingSettings.RequestTimeoutInSeconds = 10;
            });
            services.AddSingleton<TestCityGeocodingClient>();
            services.AddSingleton<ICityGeocodingClient>(serviceProvider => serviceProvider.GetRequiredService<TestCityGeocodingClient>());

            _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

            using var serviceScope = _serviceProvider.CreateScope();
            serviceScope.ServiceProvider.GetRequiredService<FMS_DbContext>().Database.EnsureCreated();
        }

        public TestCurrentUserScope CurrentUserScope => _serviceProvider.GetRequiredService<TestCurrentUserScope>();

        public TSingletonService GetRequiredSingletonService<TSingletonService>() where TSingletonService : notnull
        {
            return _serviceProvider.GetRequiredService<TSingletonService>();
        }

        public async Task RunInNewScopeAsync<TService>(Func<TService, Task> serviceCall) where TService : notnull
        {
            using var serviceScope = _serviceProvider.CreateScope();
            await serviceCall(serviceScope.ServiceProvider.GetRequiredService<TService>());
        }

        public async Task<TResult> RunInNewScopeAsync<TService, TResult>(Func<TService, Task<TResult>> serviceCall) where TService : notnull
        {
            using var serviceScope = _serviceProvider.CreateScope();
            return await serviceCall(serviceScope.ServiceProvider.GetRequiredService<TService>());
        }

        public async Task SeedAsync(params object[] seededEntities)
        {
            using var serviceScope = _serviceProvider.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<FMS_DbContext>();
            dbContext.AddRange(seededEntities);
            await dbContext.SaveChangesAsync();
        }

        public async Task<AutoStatus> GetPersistedAutoStatusAsync(int autoId)
        {
            using var serviceScope = _serviceProvider.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<FMS_DbContext>();
            return await dbContext.Set<Auto>().Where(auto => auto.Id == autoId).Select(auto => auto.Status).SingleAsync();
        }

        public static Auto CreateAuto(string number, AutoStatus status = AutoStatus.Available, string mark = "Volvo", int depotId = DepotConstraints.DefaultDepotId)
        {
            return new Auto { Mark = mark, Model = "FH16", Color = "Білий", Number = number, Capacity = 20, Status = status, DepotId = depotId };
        }

        public static Driver CreateDriver(string licenseNumber, string surname = "Шевченко", int depotId = DepotConstraints.DefaultDepotId)
        {
            return new Driver { Name = "Тарас", Surname = surname, Patronymic = "Григорович", LicenseNumber = licenseNumber, DepotId = depotId };
        }

        public static Route CreateRoute(Auto auto, Driver driver, DateTime departure, DateTime arrival, RouteStatus status = RouteStatus.Planned)
        {
            return new Route
            {
                Auto = auto,
                Driver = driver,
                DepotId = auto.DepotId,
                StartLocationId = StartLocationId,
                DestinationLocationId = DestinationLocationId,
                DepartureTimeUtc = DateTime.SpecifyKind(departure, DateTimeKind.Utc),
                ArrivalTimeUtc = DateTime.SpecifyKind(arrival, DateTimeKind.Utc),
                Status = status
            };
        }

        public static AutoMaintenance CreateMaintenance(Auto auto, DateTime serviceDate, bool isCompleted = false)
        {
            return new AutoMaintenance { Auto = auto, Name = "ТО-1", Description = "Заміна мастила", ServiceDate = serviceDate, IsCompleted = isCompleted };
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
            _sqliteConnection.Dispose();
        }
    }
}
