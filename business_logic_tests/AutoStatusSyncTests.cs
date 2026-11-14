using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class AutoStatusSyncTests : IDisposable
    {
        private static readonly DateTime RouteDeparture = new(2030, 3, 1, 8, 0, 0);
        private static readonly DateTime RouteArrival = new(2030, 3, 1, 18, 0, 0);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task UpdateAsync_RouteMovedToInProgress_SetsAutoInService()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateEditRouteModelFor(route.Id, auto.Id, driver.Id, RouteStatus.InProgress)));

            Assert.Equal(AutoStatus.InService, await _testHost.GetPersistedAutoStatusAsync(auto.Id));
        }

        [Theory]
        [InlineData(RouteStatus.Completed)]
        [InlineData(RouteStatus.Cancelled)]
        public async Task UpdateAsync_InProgressRouteFinished_SetsAutoAvailable(RouteStatus finishedRouteStatus)
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateEditRouteModelFor(route.Id, auto.Id, driver.Id, finishedRouteStatus)));

            Assert.Equal(AutoStatus.Available, await _testHost.GetPersistedAutoStatusAsync(auto.Id));
        }

        [Fact]
        public async Task UpdateAsync_InProgressRouteCompletedWithDueMaintenance_SetsAutoUnderMaintenance()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress);
            await _testHost.SeedAsync(auto, driver, route,
                BusinessLogicTestHost.CreateMaintenance(auto, DateTimeOffset.Now.Date));

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateEditRouteModelFor(route.Id, auto.Id, driver.Id, RouteStatus.Completed)));

            Assert.Equal(AutoStatus.UnderMaintenance, await _testHost.GetPersistedAutoStatusAsync(auto.Id));
        }

        [Fact]
        public async Task UpdateAsync_RouteCompletedWhileAnotherRouteInProgress_KeepsAutoInService()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var otherDriver = BusinessLogicTestHost.CreateDriver("ABC 222222");
            var completedRoute = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress);
            var stillInProgressRoute = BusinessLogicTestHost.CreateRoute(auto, otherDriver, RouteArrival, RouteArrival.AddHours(4), RouteStatus.InProgress);
            await _testHost.SeedAsync(auto, driver, otherDriver, completedRoute, stillInProgressRoute);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateEditRouteModelFor(completedRoute.Id, auto.Id, driver.Id, RouteStatus.Completed)));

            Assert.Equal(AutoStatus.InService, await _testHost.GetPersistedAutoStatusAsync(auto.Id));
        }

        [Fact]
        public async Task UpdateAsync_InProgressRouteReassignedToOtherAuto_ReleasesPreviousAuto()
        {
            var previousAuto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var replacementAuto = BusinessLogicTestHost.CreateAuto("АА2222АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(previousAuto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress);
            await _testHost.SeedAsync(previousAuto, replacementAuto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateEditRouteModelFor(route.Id, replacementAuto.Id, driver.Id, RouteStatus.InProgress)));

            Assert.Equal(AutoStatus.Available, await _testHost.GetPersistedAutoStatusAsync(previousAuto.Id));
            Assert.Equal(AutoStatus.InService, await _testHost.GetPersistedAutoStatusAsync(replacementAuto.Id));
        }

        [Fact]
        public async Task SyncAutosWithDueMaintenanceAsync_IncompleteMaintenanceDueToday_SetsAutoUnderMaintenance()
        {
            var autoWithDueMaintenance = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var autoWithFutureMaintenance = BusinessLogicTestHost.CreateAuto("АА2222АА");
            var autoWithCompletedMaintenance = BusinessLogicTestHost.CreateAuto("АА3333АА");
            await _testHost.SeedAsync(autoWithDueMaintenance, autoWithFutureMaintenance, autoWithCompletedMaintenance,
                BusinessLogicTestHost.CreateMaintenance(autoWithDueMaintenance, DateTimeOffset.Now.Date),
                BusinessLogicTestHost.CreateMaintenance(autoWithFutureMaintenance, DateTimeOffset.Now.Date.AddDays(3)),
                BusinessLogicTestHost.CreateMaintenance(autoWithCompletedMaintenance, DateTimeOffset.Now.Date, isCompleted: true));

            var autosMovedUnderMaintenanceCount = await _testHost.RunInNewScopeAsync<IAutoStatusSyncService, int>(
                autoStatusSyncService => autoStatusSyncService.SyncAutosWithDueMaintenanceAsync());

            Assert.Equal(1, autosMovedUnderMaintenanceCount);
            Assert.Equal(AutoStatus.UnderMaintenance, await _testHost.GetPersistedAutoStatusAsync(autoWithDueMaintenance.Id));
            Assert.Equal(AutoStatus.Available, await _testHost.GetPersistedAutoStatusAsync(autoWithFutureMaintenance.Id));
            Assert.Equal(AutoStatus.Available, await _testHost.GetPersistedAutoStatusAsync(autoWithCompletedMaintenance.Id));
        }

        [Fact]
        public async Task SyncAutosWithDueMaintenanceAsync_AutoOnInProgressRoute_KeepsAutoInService()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver,
                BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress),
                BusinessLogicTestHost.CreateMaintenance(auto, DateTimeOffset.Now.Date));

            await _testHost.RunInNewScopeAsync<IAutoStatusSyncService, int>(
                autoStatusSyncService => autoStatusSyncService.SyncAutosWithDueMaintenanceAsync());

            Assert.Equal(AutoStatus.InService, await _testHost.GetPersistedAutoStatusAsync(auto.Id));
        }

        [Fact]
        public async Task UpdateAsync_DueMaintenanceCompleted_SetsAutoAvailable()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.UnderMaintenance);
            var maintenance = BusinessLogicTestHost.CreateMaintenance(auto, DateTimeOffset.Now.Date);
            await _testHost.SeedAsync(auto, maintenance);

            await _testHost.RunInNewScopeAsync<IAutoMaintenanceService>(maintenanceService => maintenanceService.UpdateAsync(new EditAutoMaintenanceModel
            {
                Id = maintenance.Id,
                AutoId = auto.Id,
                Name = maintenance.Name,
                Description = maintenance.Description,
                ServiceDate = maintenance.ServiceDate,
                IsCompleted = true
            }));

            Assert.Equal(AutoStatus.Available, await _testHost.GetPersistedAutoStatusAsync(auto.Id));
        }

        [Fact]
        public async Task CreateAsync_FutureMaintenance_KeepsAutoAvailable()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            await _testHost.SeedAsync(auto);

            await _testHost.RunInNewScopeAsync<IAutoMaintenanceService, AutoMaintenanceDTO>(maintenanceService => maintenanceService.CreateAsync(new CreateAutoMaintenanceModel
            {
                AutoId = auto.Id,
                Name = "ТО-2",
                Description = "Заміна фільтрів",
                ServiceDate = DateTimeOffset.Now.Date.AddDays(5)
            }));

            Assert.Equal(AutoStatus.Available, await _testHost.GetPersistedAutoStatusAsync(auto.Id));
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
