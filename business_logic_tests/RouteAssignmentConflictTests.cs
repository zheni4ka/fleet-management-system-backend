using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class RouteAssignmentConflictTests : IDisposable
    {
        private static readonly DateTime ExistingRouteDeparture = new(2030, 1, 10, 8, 0, 0);
        private static readonly DateTime ExistingRouteArrival = new(2030, 1, 10, 18, 0, 0);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task CreateAsync_AutoAssignedToOverlappingActiveRoute_ThrowsAssignmentConflict()
        {
            var busyAuto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var busyDriver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var freeDriver = BusinessLogicTestHost.CreateDriver("ABC 222222");
            await _testHost.SeedAsync(busyAuto, busyDriver, freeDriver,
                BusinessLogicTestHost.CreateRoute(busyAuto, busyDriver, ExistingRouteDeparture, ExistingRouteArrival));

            var conflictException = await Assert.ThrowsAsync<AssignmentConflictException>(() => _testHost.RunInNewScopeAsync<IRouteService>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(busyAuto.Id, freeDriver.Id, ExistingRouteDeparture.AddHours(2), ExistingRouteArrival.AddHours(2)))));

            Assert.Contains($"ID {busyAuto.Id}", conflictException.Message);
        }

        [Fact]
        public async Task CreateAsync_DriverAssignedToOverlappingActiveRoute_ThrowsAssignmentConflict()
        {
            var busyAuto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var freeAuto = BusinessLogicTestHost.CreateAuto("АА2222АА");
            var busyDriver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(busyAuto, freeAuto, busyDriver,
                BusinessLogicTestHost.CreateRoute(busyAuto, busyDriver, ExistingRouteDeparture, ExistingRouteArrival, RouteStatus.InProgress));

            var conflictException = await Assert.ThrowsAsync<AssignmentConflictException>(() => _testHost.RunInNewScopeAsync<IRouteService>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(freeAuto.Id, busyDriver.Id, ExistingRouteDeparture.AddHours(-2), ExistingRouteDeparture.AddHours(1)))));

            Assert.Contains($"ID {busyDriver.Id}", conflictException.Message);
        }

        [Theory]
        [InlineData(RouteStatus.Completed)]
        [InlineData(RouteStatus.Cancelled)]
        public async Task CreateAsync_OverlappingRouteIsNotActive_CreatesRoute(RouteStatus inactiveRouteStatus)
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver,
                BusinessLogicTestHost.CreateRoute(auto, driver, ExistingRouteDeparture, ExistingRouteArrival, inactiveRouteStatus));

            var createdRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, ExistingRouteDeparture, ExistingRouteArrival)));

            Assert.True(createdRoute.Id > 0);
        }

        [Fact]
        public async Task CreateAsync_RouteDepartsWhenExistingRouteArrives_CreatesRoute()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver,
                BusinessLogicTestHost.CreateRoute(auto, driver, ExistingRouteDeparture, ExistingRouteArrival));

            var createdRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, ExistingRouteArrival, ExistingRouteArrival.AddHours(5))));

            Assert.True(createdRoute.Id > 0);
        }

        [Fact]
        public async Task CreateAsync_IncompleteMaintenanceDuringRoute_ThrowsAssignmentConflict()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver,
                BusinessLogicTestHost.CreateMaintenance(auto, ExistingRouteDeparture.Date));

            var conflictException = await Assert.ThrowsAsync<AssignmentConflictException>(() => _testHost.RunInNewScopeAsync<IRouteService>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, ExistingRouteDeparture, ExistingRouteArrival))));

            Assert.Contains("техобслуговування", conflictException.Message);
        }

        [Fact]
        public async Task UpdateAsync_CancelRouteThatOverlapsAnotherActiveRoute_UpdatesRoute()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var firstRoute = BusinessLogicTestHost.CreateRoute(auto, driver, ExistingRouteDeparture, ExistingRouteArrival);
            var overlappingRoute = BusinessLogicTestHost.CreateRoute(auto, driver, ExistingRouteDeparture.AddHours(1), ExistingRouteArrival.AddHours(1));
            await _testHost.SeedAsync(auto, driver, firstRoute, overlappingRoute);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(overlappingRoute.Id, auto.Id, driver.Id, RouteStatus.Cancelled)));

            var updatedRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.GetAsync(overlappingRoute.Id));
            Assert.Equal(RouteStatus.Cancelled, updatedRoute.Status);
        }

        [Fact]
        public async Task UpdateAsync_ReactivateCancelledRouteThatOverlapsActiveRoute_ThrowsAssignmentConflict()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var activeRoute = BusinessLogicTestHost.CreateRoute(auto, driver, ExistingRouteDeparture, ExistingRouteArrival);
            var cancelledRoute = BusinessLogicTestHost.CreateRoute(auto, driver, ExistingRouteDeparture, ExistingRouteArrival, RouteStatus.Cancelled);
            await _testHost.SeedAsync(auto, driver, activeRoute, cancelledRoute);

            await Assert.ThrowsAsync<AssignmentConflictException>(() => _testHost.RunInNewScopeAsync<IRouteService>(
                routeService => routeService.UpdateAsync(RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(cancelledRoute.Id, auto.Id, driver.Id, RouteStatus.Planned))));
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
