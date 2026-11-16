using business_logic;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic.Settings;
using data_access;
using data_access.data;
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
            services.AddDbContext<FMS_DbContext>(options => options.UseSqlite(_sqliteConnection));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUserService, TestCurrentUserService>();
            services.AddScoped<ITokenService, TestTokenService>();
            services.AddAuthentication();
            services.AddAccountIdentity();
            services.AddOptions<RefreshTokenSettings>().Configure(refreshTokenSettings =>
            {
                refreshTokenSettings.ExpiryInDays = TestRefreshTokenExpiryInDays;
                refreshTokenSettings.RotatedTokenReuseGracePeriodInSeconds = rotatedTokenReuseGracePeriodInSeconds;
            });

            _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

            using var serviceScope = _serviceProvider.CreateScope();
            serviceScope.ServiceProvider.GetRequiredService<FMS_DbContext>().Database.EnsureCreated();
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

        public static Auto CreateAuto(string number, AutoStatus status = AutoStatus.Available, string mark = "Volvo")
        {
            return new Auto { Mark = mark, Model = "FH16", Color = "Білий", Number = number, Capacity = 20, Status = status };
        }

        public static Driver CreateDriver(string licenseNumber, string surname = "Шевченко")
        {
            return new Driver { Name = "Тарас", Surname = surname, Patronymic = "Григорович", LicenseNumber = licenseNumber };
        }

        public static Route CreateRoute(Auto auto, Driver driver, DateTime departure, DateTime arrival, RouteStatus status = RouteStatus.Planned)
        {
            return new Route
            {
                Auto = auto,
                Driver = driver,
                StartLocationId = StartLocationId,
                DestinationLocationId = DestinationLocationId,
                DepartureTime = departure,
                ArrivalTime = arrival,
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
