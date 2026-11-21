using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Helpers;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using FluentValidation;

namespace business_logic_tests
{
    public sealed class MaintenanceIntervalTests : IDisposable
    {
        private static readonly DateTime RouteDeparture = new(2030, 1, 11, 8, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime RouteArrival = new(2030, 1, 11, 18, 0, 0, DateTimeKind.Utc);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task CreateAsync_RouteDayAfterOpenEndedMaintenanceStarted_ThrowsAssignmentConflict()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver,
                BusinessLogicTestHost.CreateMaintenance(auto, new DateTime(2030, 1, 10, 9, 0, 0)));

            await Assert.ThrowsAsync<AssignmentConflictException>(() => _testHost.RunInNewScopeAsync<IRouteService>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, RouteDeparture, RouteArrival))));
        }

        [Fact]
        public async Task CreateAsync_MaintenancePlannedEndInsideRoute_ThrowsAssignmentConflict()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver,
                BusinessLogicTestHost.CreateMaintenance(auto, new DateTime(2030, 1, 10, 9, 0, 0), plannedEndDate: new DateTime(2030, 1, 11, 14, 0, 0)));

            await Assert.ThrowsAsync<AssignmentConflictException>(() => _testHost.RunInNewScopeAsync<IRouteService>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, RouteDeparture, RouteArrival))));
        }

        [Fact]
        public async Task CreateAsync_RouteAfterMaintenancePlannedEnd_CreatesRoute()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver,
                BusinessLogicTestHost.CreateMaintenance(auto, new DateTime(2030, 1, 9, 9, 0, 0), plannedEndDate: new DateTime(2030, 1, 10, 18, 0, 0)));

            var createdRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, RouteDeparture, RouteArrival)));

            Assert.True(createdRoute.Id > 0);
        }

        [Fact]
        public async Task CreateAsync_CompletedOpenEndedMaintenanceBeforeRoute_CreatesRoute()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver,
                BusinessLogicTestHost.CreateMaintenance(auto, new DateTime(2030, 1, 10, 9, 0, 0), isCompleted: true));

            var createdRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, RouteDeparture, RouteArrival)));

            Assert.True(createdRoute.Id > 0);
        }

        [Fact]
        public async Task MaintenanceCreateAsync_WithPlannedEnd_CancelsOnlyOverlappingRoutes()
        {
            var maintenanceStartBusinessDateTime = BusinessTimeZoneConverter.GetBusinessToday().AddDays(10).ToDateTime(new TimeOnly(9, 0));
            var maintenanceStartUtc = BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(maintenanceStartBusinessDateTime);
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var overlappingRoute = BusinessLogicTestHost.CreateRoute(auto, driver, maintenanceStartUtc.AddHours(2), maintenanceStartUtc.AddHours(6));
            var routeAfterPlannedEnd = BusinessLogicTestHost.CreateRoute(auto, driver, maintenanceStartUtc.AddDays(3), maintenanceStartUtc.AddDays(3).AddHours(6));
            await _testHost.SeedAsync(auto, driver, overlappingRoute, routeAfterPlannedEnd);

            await _testHost.RunInNewScopeAsync<IAutoMaintenanceService, GetAutoMaintenanceResponseModel>(maintenanceService => maintenanceService.CreateAsync(
                CreateMaintenanceRequestModelFor(auto.Id, maintenanceStartBusinessDateTime, maintenanceStartBusinessDateTime.AddDays(1))));

            Assert.Equal(RouteStatus.Cancelled, await GetPersistedRouteStatusAsync(overlappingRoute.Id));
            Assert.Equal(RouteStatus.Planned, await GetPersistedRouteStatusAsync(routeAfterPlannedEnd.Id));
        }

        [Fact]
        public async Task MaintenanceCreateAsync_PlannedEndBeforeStart_ThrowsValidation()
        {
            var maintenanceStartBusinessDateTime = BusinessTimeZoneConverter.GetBusinessToday().AddDays(10).ToDateTime(new TimeOnly(9, 0));
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            await _testHost.SeedAsync(auto);

            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<IAutoMaintenanceService, GetAutoMaintenanceResponseModel>(
                maintenanceService => maintenanceService.CreateAsync(
                    CreateMaintenanceRequestModelFor(auto.Id, maintenanceStartBusinessDateTime, maintenanceStartBusinessDateTime.AddHours(-1)))));
        }

        private static CreateAutoMaintenanceRequestModel CreateMaintenanceRequestModelFor(int autoId, DateTime serviceDate, DateTime? plannedEndDate)
        {
            return new CreateAutoMaintenanceRequestModel
            {
                AutoId = autoId,
                Name = "ТО-2",
                Description = "Заміна фільтрів",
                ServiceDate = serviceDate,
                PlannedEndDate = plannedEndDate
            };
        }

        private async Task<RouteStatus> GetPersistedRouteStatusAsync(int routeId)
        {
            var persistedRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.GetAsync(routeId));
            return persistedRoute.Status;
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
