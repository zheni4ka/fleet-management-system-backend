using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class RoutesPeriodReportOverlapTests : IDisposable
    {
        private static readonly DateOnly ReportBusinessDate = new(2030, 6, 10);
        private static readonly DateTime PeriodStartUtc = new(2030, 6, 9, 21, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime PeriodEndExclusiveUtc = new(2030, 6, 10, 21, 0, 0, DateTimeKind.Utc);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task GetRoutesPeriodReportAsync_RoutesCrossingPeriodBoundaries_IncludesThemAndClipsHours()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var routeCrossingPeriodStart = BusinessLogicTestHost.CreateRoute(auto, driver,
                PeriodStartUtc.AddHours(-3), PeriodStartUtc.AddHours(6), RouteStatus.Completed);
            var routeInsidePeriod = BusinessLogicTestHost.CreateRoute(auto, driver,
                PeriodStartUtc.AddHours(11), PeriodStartUtc.AddHours(15), RouteStatus.Completed);
            var routeCrossingMidnightPastPeriodEnd = BusinessLogicTestHost.CreateRoute(auto, driver,
                PeriodEndExclusiveUtc.AddHours(-2), PeriodEndExclusiveUtc.AddHours(4), RouteStatus.Completed);
            await _testHost.SeedAsync(auto, driver, routeCrossingPeriodStart, routeInsidePeriod, routeCrossingMidnightPastPeriodEnd);

            var periodReport = await GetOneDayPeriodReportAsync();

            Assert.Equal(
                new[] { routeCrossingPeriodStart.Id, routeInsidePeriod.Id, routeCrossingMidnightPastPeriodEnd.Id },
                periodReport.Routes.Select(route => route.Id));
            Assert.Equal(3, periodReport.CompletedRoutesCount);
            Assert.Equal(12, periodReport.CompletedRoutesDrivingHours);
            Assert.Equal(1, periodReport.InvolvedAutosCount);
            var driverSummary = Assert.Single(periodReport.DriverSummaries);
            Assert.Equal(3, driverSummary.RoutesCount);
            Assert.Equal(3, driverSummary.CompletedRoutesCount);
            Assert.Equal(12, driverSummary.CompletedRoutesDrivingHours);
        }

        [Fact]
        public async Task GetRoutesPeriodReportAsync_RoutesTouchingPeriodBoundaries_ExcludesThem()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver,
                BusinessLogicTestHost.CreateRoute(auto, driver, PeriodStartUtc.AddHours(-6), PeriodStartUtc, RouteStatus.Completed),
                BusinessLogicTestHost.CreateRoute(auto, driver, PeriodEndExclusiveUtc, PeriodEndExclusiveUtc.AddHours(5)));

            var periodReport = await GetOneDayPeriodReportAsync();

            Assert.Equal(0, periodReport.TotalRoutesCount);
            Assert.Equal(0, periodReport.InvolvedAutosCount);
            Assert.Empty(periodReport.DriverSummaries);
        }

        [Fact]
        public async Task GetRoutesPeriodReportAsync_ActualArrivalLateIntoPeriod_IncludesRouteWithActualHoursInsidePeriod()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var lateRoute = BusinessLogicTestHost.CreateRoute(auto, driver,
                PeriodStartUtc.AddHours(-6), PeriodStartUtc.AddHours(-1), RouteStatus.Completed);
            lateRoute.ActualDepartureTimeUtc = PeriodStartUtc.AddHours(-5);
            lateRoute.ActualArrivalTimeUtc = PeriodStartUtc.AddHours(2);
            await _testHost.SeedAsync(auto, driver, lateRoute);

            var periodReport = await GetOneDayPeriodReportAsync();

            Assert.Equal(lateRoute.Id, Assert.Single(periodReport.Routes).Id);
            Assert.Equal(2, periodReport.CompletedRoutesDrivingHours);
        }

        private Task<GetRoutesPeriodReportResponseModel> GetOneDayPeriodReportAsync()
        {
            return _testHost.RunInNewScopeAsync<IRouteService, GetRoutesPeriodReportResponseModel>(
                routeService => routeService.GetRoutesPeriodReportAsync(new GetRoutesPeriodReportRequestModel(
                    PeriodStartDate: ReportBusinessDate,
                    PeriodEndDate: ReportBusinessDate)));
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
