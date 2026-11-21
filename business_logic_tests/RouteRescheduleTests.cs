using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using FluentValidation;

namespace business_logic_tests
{
    public sealed class RouteRescheduleTests : IDisposable
    {
        private static readonly DateTime RouteDeparture = new(2030, 8, 1, 8, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime RouteArrival = new(2030, 8, 1, 18, 0, 0, DateTimeKind.Utc);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task UpdateAsync_PlannedRouteMovedToFreeInterval_UpdatesRouteTimes()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                CreateRescheduleRequestModel(route, auto.Id, driver.Id, RouteStatus.Planned, RouteDeparture.AddDays(1), RouteArrival.AddDays(1))));

            var rescheduledRoute = await GetRouteAsync(route.Id);
            Assert.Equal(RouteDeparture.AddDays(1), rescheduledRoute.DepartureTimeUtc);
            Assert.Equal(RouteArrival.AddDays(1), rescheduledRoute.ArrivalTimeUtc);
        }

        [Fact]
        public async Task UpdateAsync_PlannedRouteMovedOntoBusyAutoInterval_ThrowsAssignmentConflictAndKeepsTimes()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var otherDriver = BusinessLogicTestHost.CreateDriver("ABC 222222");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            var busyAutoRoute = BusinessLogicTestHost.CreateRoute(auto, otherDriver, RouteDeparture.AddDays(1), RouteArrival.AddDays(1));
            await _testHost.SeedAsync(auto, driver, otherDriver, route, busyAutoRoute);

            await Assert.ThrowsAsync<AssignmentConflictException>(() => _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                CreateRescheduleRequestModel(route, auto.Id, driver.Id, RouteStatus.Planned, RouteDeparture.AddDays(1).AddHours(2), RouteArrival.AddDays(1).AddHours(2)))));

            var unchangedRoute = await GetRouteAsync(route.Id);
            Assert.Equal(RouteDeparture, unchangedRoute.DepartureTimeUtc);
            Assert.Equal(RouteArrival, unchangedRoute.ArrivalTimeUtc);
        }

        [Fact]
        public async Task UpdateAsync_PlannedRouteMovedOntoIncompleteMaintenance_ThrowsAssignmentConflict()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            await _testHost.SeedAsync(auto, driver, route,
                BusinessLogicTestHost.CreateMaintenance(auto, new DateTime(2030, 8, 5, 9, 0, 0), plannedEndDate: new DateTime(2030, 8, 6, 18, 0, 0)));

            await Assert.ThrowsAsync<AssignmentConflictException>(() => _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                CreateRescheduleRequestModel(route, auto.Id, driver.Id, RouteStatus.Planned, RouteDeparture.AddDays(4), RouteArrival.AddDays(4)))));
        }

        [Fact]
        public async Task UpdateAsync_InProgressRouteRescheduled_ThrowsBusinessRule()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress);
            await _testHost.SeedAsync(auto, driver, route);

            await Assert.ThrowsAsync<BusinessRuleException>(() => _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                CreateRescheduleRequestModel(route, auto.Id, driver.Id, RouteStatus.InProgress, RouteDeparture, RouteArrival.AddHours(3)))));
        }

        [Fact]
        public async Task UpdateAsync_InProgressRouteWithUnchangedTimes_UpdatesRoute()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                CreateRescheduleRequestModel(route, auto.Id, driver.Id, RouteStatus.Completed, RouteDeparture, RouteArrival)));

            Assert.Equal(RouteStatus.Completed, (await GetRouteAsync(route.Id)).Status);
        }

        [Fact]
        public async Task UpdateAsync_OnlyDepartureTimeSent_ThrowsValidation()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            await _testHost.SeedAsync(auto, driver, route);

            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Planned) with
                {
                    DepartureTimeUtc = RouteDeparture.AddDays(1)
                })));
        }

        [Fact]
        public async Task UpdateAsync_DepartureAfterArrival_ThrowsValidation()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            await _testHost.SeedAsync(auto, driver, route);

            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                CreateRescheduleRequestModel(route, auto.Id, driver.Id, RouteStatus.Planned, RouteArrival, RouteDeparture))));
        }

        private static UpdateRouteRequestModel CreateRescheduleRequestModel(
            Route route, int autoId, int driverId, RouteStatus status, DateTime departureTimeUtc, DateTime arrivalTimeUtc)
        {
            return RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, autoId, driverId, status) with
            {
                DepartureTimeUtc = departureTimeUtc,
                ArrivalTimeUtc = arrivalTimeUtc
            };
        }

        private Task<GetRouteResponseModel> GetRouteAsync(int routeId)
        {
            return _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.GetAsync(routeId));
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
