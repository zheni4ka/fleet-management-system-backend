using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Helpers;
using business_logic.Interfaces;
using business_logic.Validators;
using business_logic.Validators.Auto;
using business_logic_tests.Infrastructure;
using data_access.data;
using data_access.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace business_logic_tests
{
    public sealed class DemoDataSeederTests : IDisposable
    {
        private const string TestAdminPassword = "Valid1AdminPassword";
        private const string TestDispatcherPassword = "Valid1DispatcherPassword";
        private static readonly DateTime SeedNowUtc = new(2030, 9, 15, 10, 0, 0, DateTimeKind.Utc);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task SeedAsync_EmptyFleet_CreatesDemoCatalogAndHistory()
        {
            var demoDataSeedSummary = await SeedDemoDataAsync();

            Assert.Equal(3, demoDataSeedSummary.DepotsCount);
            Assert.Equal(12, demoDataSeedSummary.AutosCount);
            Assert.Equal(15, demoDataSeedSummary.DriversCount);
            Assert.Equal(3, demoDataSeedSummary.DispatchersCount);
            Assert.Equal(25, demoDataSeedSummary.LocationsCount);
            Assert.Equal(4, demoDataSeedSummary.MaintenancesCount);
            Assert.InRange(demoDataSeedSummary.RoutesCount, 120, 190);
            Assert.True(demoDataSeedSummary.AuditLogsCount > demoDataSeedSummary.RoutesCount * 2);
            var completedRoutesCount = await ReadAsync(dbContext => dbContext.Set<Route>().CountAsync(route => route.Status == RouteStatus.Completed));
            Assert.Equal(completedRoutesCount, demoDataSeedSummary.DriverTripsCount);
        }

        [Fact]
        public async Task SeedAsync_EmptyFleet_HasNoOverlappingActiveAssignments()
        {
            await SeedDemoDataAsync();

            var routes = await ReadAsync(dbContext => dbContext.Set<Route>().ToListAsync());
            var activeRoutes = routes.Where(route => route.Status != RouteStatus.Cancelled).ToList();
            foreach (var route in activeRoutes)
            {
                var overlappingRoute = activeRoutes.FirstOrDefault(otherRoute => otherRoute.Id != route.Id
                    && (otherRoute.AutoId == route.AutoId || otherRoute.DriverId == route.DriverId)
                    && otherRoute.DepartureTimeUtc < route.ArrivalTimeUtc
                    && otherRoute.ArrivalTimeUtc > route.DepartureTimeUtc);
                Assert.Null(overlappingRoute);
            }

            Assert.Contains(routes, route => route.Status == RouteStatus.Completed);
            Assert.Contains(routes, route => route.Status == RouteStatus.Planned);
            Assert.Contains(routes, route => route.Status == RouteStatus.Cancelled);
            Assert.All(routes.Where(route => route.Status == RouteStatus.Completed), completedRoute =>
            {
                Assert.NotNull(completedRoute.ActualDepartureTimeUtc);
                Assert.NotNull(completedRoute.ActualArrivalTimeUtc);
                Assert.True(completedRoute.ActualArrivalTimeUtc <= SeedNowUtc);
            });
        }

        [Fact]
        public async Task SeedAsync_EmptyFleet_KeepsRoutesOutOfMaintenanceAndMarksMaintainedAuto()
        {
            await SeedDemoDataAsync();

            var maintenances = await ReadAsync(dbContext => dbContext.Set<AutoMaintenance>().ToListAsync());
            var routes = await ReadAsync(dbContext => dbContext.Set<Route>().ToListAsync());
            foreach (var maintenance in maintenances)
            {
                var maintenanceStartUtc = BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(maintenance.ServiceDate);
                var maintenanceEndUtc = maintenance.PlannedEndDate.HasValue
                    ? BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(maintenance.PlannedEndDate.Value)
                    : DateTime.MaxValue;
                Assert.DoesNotContain(routes, route => route.AutoId == maintenance.AutoId
                    && route.DepartureTimeUtc < maintenanceEndUtc
                    && route.ArrivalTimeUtc > maintenanceStartUtc);
            }

            var openEndedMaintenance = Assert.Single(maintenances, maintenance => !maintenance.IsCompleted && !maintenance.PlannedEndDate.HasValue);
            Assert.Equal(AutoStatus.UnderMaintenance, await _testHost.GetPersistedAutoStatusAsync(openEndedMaintenance.AutoId));
        }

        [Fact]
        public async Task SeedAsync_EmptyFleet_CatalogPassesRequestValidators()
        {
            await SeedDemoDataAsync();

            var autos = await ReadAsync(dbContext => dbContext.Set<Auto>().ToListAsync());
            var drivers = await ReadAsync(dbContext => dbContext.Set<Driver>().ToListAsync());
            var createAutoRequestModelValidator = new CreateAutoRequestModelValidator();
            var createDriverRequestModelValidator = new CreateDriverRequestModelValidator();
            Assert.All(autos, auto => Assert.True(createAutoRequestModelValidator.Validate(new CreateAutoRequestModel
            {
                Mark = auto.Mark,
                Model = auto.Model,
                Color = auto.Color,
                Number = auto.Number,
                Capacity = auto.Capacity,
                DepotId = auto.DepotId
            }).IsValid, auto.Number));
            Assert.All(drivers, driver => Assert.True(createDriverRequestModelValidator.Validate(new CreateDriverRequestModel
            {
                Name = driver.Name,
                Surname = driver.Surname,
                Patronymic = driver.Patronymic,
                LicenseNumber = driver.LicenseNumber,
                PhoneNumber = driver.PhoneNumber,
                Email = driver.Email,
                DepotId = driver.DepotId
            }).IsValid, driver.LicenseNumber));
        }

        [Fact]
        public async Task SeedAsync_EmptyFleet_ProducesPeriodReportWithDrivingHours()
        {
            await SeedDemoDataAsync();
            var seedBusinessToday = DateOnly.FromDateTime(BusinessTimeZoneConverter.ConvertUtcToBusinessDateTime(SeedNowUtc));

            var periodReport = await _testHost.RunInNewScopeAsync<IRouteService, GetRoutesPeriodReportResponseModel>(
                routeService => routeService.GetRoutesPeriodReportAsync(new GetRoutesPeriodReportRequestModel(
                    PeriodStartDate: seedBusinessToday.AddDays(-30),
                    PeriodEndDate: seedBusinessToday)));

            Assert.True(periodReport.CompletedRoutesCount > 20);
            Assert.True(periodReport.CompletedRoutesDrivingHours > 100);
            Assert.Equal(15, periodReport.InvolvedDriversCount);
        }

        [Fact]
        public async Task SeedAsync_DemoDispatcher_CanLogIn()
        {
            await SeedDemoDataAsync();

            var loginResponse = await _testHost.RunInNewScopeAsync<IAccountService, LoginResponseModel>(accountService => accountService.LoginAsync(
                new LoginRequestModel { Username = DemoDataCatalog.Dispatchers[0].UserName, Password = TestDispatcherPassword }));

            Assert.False(string.IsNullOrEmpty(loginResponse.RefreshToken));
        }

        [Fact]
        public async Task SeedAsync_FleetAlreadyHasAutos_ThrowsWithoutChanges()
        {
            await SeedDemoDataAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() => DemoDataSeeder.SeedAsync(
                _testHost.Services, new DemoSeedSettings { DispatcherPassword = TestDispatcherPassword }, SeedNowUtc));

            Assert.Equal(12, await ReadAsync(dbContext => dbContext.Set<Auto>().CountAsync()));
        }

        [Fact]
        public async Task SeedAsync_DispatcherPasswordMissing_Throws()
        {
            var missingPasswordException = await Assert.ThrowsAsync<InvalidOperationException>(() => DemoDataSeeder.SeedAsync(
                _testHost.Services, new DemoSeedSettings(), SeedNowUtc));

            Assert.Contains($"{DemoSeedSettings.SectionName}:{nameof(DemoSeedSettings.DispatcherPassword)}", missingPasswordException.Message);
        }

        private async Task<DemoDataSeedSummary> SeedDemoDataAsync()
        {
            await IdentitySeeder.SeedAsync(_testHost.Services, new SeedAdminSettings { Password = TestAdminPassword });
            return await DemoDataSeeder.SeedAsync(_testHost.Services, new DemoSeedSettings { DispatcherPassword = TestDispatcherPassword }, SeedNowUtc);
        }

        private async Task<TResult> ReadAsync<TResult>(Func<FMS_DbContext, Task<TResult>> databaseRead)
        {
            using var serviceScope = _testHost.Services.CreateScope();
            return await databaseRead(serviceScope.ServiceProvider.GetRequiredService<FMS_DbContext>());
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
