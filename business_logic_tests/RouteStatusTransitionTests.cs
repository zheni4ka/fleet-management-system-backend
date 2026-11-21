using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class RouteStatusTransitionTests : IDisposable
    {
        private static readonly DateTime RouteDeparture = new(2030, 4, 1, 8, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime RouteArrival = new(2030, 4, 1, 18, 0, 0, DateTimeKind.Utc);

        private readonly BusinessLogicTestHost _testHost = new();

        [Theory]
        [InlineData(RouteStatus.Planned, RouteStatus.InProgress)]
        [InlineData(RouteStatus.Planned, RouteStatus.Cancelled)]
        [InlineData(RouteStatus.InProgress, RouteStatus.Completed)]
        [InlineData(RouteStatus.InProgress, RouteStatus.Cancelled)]
        [InlineData(RouteStatus.Planned, RouteStatus.Planned)]
        [InlineData(RouteStatus.Completed, RouteStatus.Completed)]
        public async Task UpdateAsync_AllowedStatusTransition_UpdatesRouteStatus(RouteStatus sourceRouteStatus, RouteStatus targetRouteStatus)
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, sourceRouteStatus);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, targetRouteStatus)));

            var updatedRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.GetAsync(route.Id));
            Assert.Equal(targetRouteStatus, updatedRoute.Status);
        }

        [Theory]
        [InlineData(RouteStatus.Planned, RouteStatus.Completed)]
        [InlineData(RouteStatus.InProgress, RouteStatus.Planned)]
        [InlineData(RouteStatus.Completed, RouteStatus.Planned)]
        [InlineData(RouteStatus.Completed, RouteStatus.InProgress)]
        [InlineData(RouteStatus.Completed, RouteStatus.Cancelled)]
        [InlineData(RouteStatus.Cancelled, RouteStatus.Planned)]
        [InlineData(RouteStatus.Cancelled, RouteStatus.InProgress)]
        [InlineData(RouteStatus.Cancelled, RouteStatus.Completed)]
        public async Task UpdateAsync_ForbiddenStatusTransition_ThrowsBusinessRuleAndKeepsStatus(RouteStatus sourceRouteStatus, RouteStatus targetRouteStatus)
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, sourceRouteStatus);
            await _testHost.SeedAsync(auto, driver, route);

            var businessRuleException = await Assert.ThrowsAsync<BusinessRuleException>(() => _testHost.RunInNewScopeAsync<IRouteService>(
                routeService => routeService.UpdateAsync(RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, targetRouteStatus))));

            Assert.Contains($"'{route.Id}'", businessRuleException.Message);
            var unchangedRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.GetAsync(route.Id));
            Assert.Equal(sourceRouteStatus, unchangedRoute.Status);
        }

        [Theory]
        [InlineData(RouteStatus.Completed)]
        [InlineData(RouteStatus.Cancelled)]
        public async Task UpdateAsync_FinalRouteAutoChanged_ThrowsBusinessRule(RouteStatus finalRouteStatus)
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var replacementAuto = BusinessLogicTestHost.CreateAuto("АА2222АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, finalRouteStatus);
            await _testHost.SeedAsync(auto, replacementAuto, driver, route);

            await Assert.ThrowsAsync<BusinessRuleException>(() => _testHost.RunInNewScopeAsync<IRouteService>(
                routeService => routeService.UpdateAsync(RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, replacementAuto.Id, driver.Id, finalRouteStatus))));

            var unchangedRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.GetAsync(route.Id));
            Assert.Equal(auto.Id, unchangedRoute.AutoId);
        }

        [Theory]
        [InlineData(RouteStatus.Completed)]
        [InlineData(RouteStatus.Cancelled)]
        public async Task UpdateAsync_FinalRouteDriverChanged_ThrowsBusinessRule(RouteStatus finalRouteStatus)
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var replacementDriver = BusinessLogicTestHost.CreateDriver("ABC 222222");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, finalRouteStatus);
            await _testHost.SeedAsync(auto, driver, replacementDriver, route);

            await Assert.ThrowsAsync<BusinessRuleException>(() => _testHost.RunInNewScopeAsync<IRouteService>(
                routeService => routeService.UpdateAsync(RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, replacementDriver.Id, finalRouteStatus))));

            var unchangedRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.GetAsync(route.Id));
            Assert.Equal(driver.Id, unchangedRoute.DriverId);
        }

        [Fact]
        public async Task UpdateAsync_InProgressRouteCancelledWithReplacementDriver_UpdatesRoute()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var replacementDriver = BusinessLogicTestHost.CreateDriver("ABC 222222");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress);
            await _testHost.SeedAsync(auto, driver, replacementDriver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, replacementDriver.Id, RouteStatus.Cancelled)));

            var updatedRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.GetAsync(route.Id));
            Assert.Equal(RouteStatus.Cancelled, updatedRoute.Status);
            Assert.Equal(replacementDriver.Id, updatedRoute.DriverId);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
