using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class RouteActualTimesTests : IDisposable
    {
        private static readonly DateTime RouteDeparture = new(2030, 5, 1, 8, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime RouteArrival = new(2030, 5, 1, 18, 0, 0, DateTimeKind.Utc);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task UpdateAsync_RouteStarted_RecordsActualDepartureOnly()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            await _testHost.SeedAsync(auto, driver, route);
            var routeStartRequestedUtc = DateTime.UtcNow;

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.InProgress)));

            var startedRoute = await GetRouteAsync(route.Id);
            Assert.NotNull(startedRoute.ActualDepartureTimeUtc);
            Assert.InRange(startedRoute.ActualDepartureTimeUtc.Value, routeStartRequestedUtc, DateTime.UtcNow);
            Assert.Equal(DateTimeKind.Utc, startedRoute.ActualDepartureTimeUtc.Value.Kind);
            Assert.Null(startedRoute.ActualArrivalTimeUtc);
        }

        [Fact]
        public async Task UpdateAsync_RouteCompleted_RecordsActualArrivalAndKeepsActualDeparture()
        {
            var actualDepartureTimeUtc = RouteDeparture.AddMinutes(25);
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress);
            route.ActualDepartureTimeUtc = actualDepartureTimeUtc;
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Completed)));

            var completedRoute = await GetRouteAsync(route.Id);
            Assert.Equal(actualDepartureTimeUtc, completedRoute.ActualDepartureTimeUtc);
            Assert.NotNull(completedRoute.ActualArrivalTimeUtc);
        }

        [Fact]
        public async Task UpdateAsync_PlannedRouteCancelled_LeavesActualTimesEmpty()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Cancelled)));

            var cancelledRoute = await GetRouteAsync(route.Id);
            Assert.Null(cancelledRoute.ActualDepartureTimeUtc);
            Assert.Null(cancelledRoute.ActualArrivalTimeUtc);
        }

        [Fact]
        public async Task GetRoutesPeriodReportAsync_CompletedRouteWithActualTimes_CountsActualDrivingHours()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var routeWithActualTimes = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.Completed);
            routeWithActualTimes.ActualDepartureTimeUtc = RouteDeparture.AddHours(1);
            routeWithActualTimes.ActualArrivalTimeUtc = RouteArrival.AddHours(2);
            var routeWithPlannedTimesOnly = BusinessLogicTestHost.CreateRoute(auto, driver,
                RouteDeparture.AddDays(1), RouteDeparture.AddDays(1).AddHours(3), RouteStatus.Completed);
            await _testHost.SeedAsync(auto, driver, routeWithActualTimes, routeWithPlannedTimesOnly);

            var periodReport = await _testHost.RunInNewScopeAsync<IRouteService, GetRoutesPeriodReportResponseModel>(
                routeService => routeService.GetRoutesPeriodReportAsync(new GetRoutesPeriodReportRequestModel(
                    PeriodStartDate: new DateOnly(2030, 5, 1),
                    PeriodEndDate: new DateOnly(2030, 5, 31))));

            Assert.Equal(14, periodReport.CompletedRoutesDrivingHours);
            Assert.Equal(14, periodReport.DriverSummaries[0].CompletedRoutesDrivingHours);
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
