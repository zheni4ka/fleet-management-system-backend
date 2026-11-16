using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class RoutesPeriodReportTests : IDisposable
    {
        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task GetRoutesPeriodReportAsync_RoutesInsideAndOutsidePeriod_AggregatesOnlyPeriodRoutes()
        {
            var firstAuto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var secondAuto = BusinessLogicTestHost.CreateAuto("АА2222АА");
            var busyDriver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var idleDriver = BusinessLogicTestHost.CreateDriver("ABC 222222", surname: "Франко");
            var firstCompletedRoute = BusinessLogicTestHost.CreateRoute(firstAuto, busyDriver,
                new DateTime(2030, 6, 1, 8, 0, 0), new DateTime(2030, 6, 1, 12, 0, 0), RouteStatus.Completed);
            var secondCompletedRoute = BusinessLogicTestHost.CreateRoute(firstAuto, busyDriver,
                new DateTime(2030, 6, 2, 8, 0, 0), new DateTime(2030, 6, 2, 10, 30, 0), RouteStatus.Completed);
            var plannedRoute = BusinessLogicTestHost.CreateRoute(secondAuto, idleDriver,
                new DateTime(2030, 6, 3, 8, 0, 0), new DateTime(2030, 6, 3, 18, 0, 0));
            var cancelledRoute = BusinessLogicTestHost.CreateRoute(secondAuto, busyDriver,
                new DateTime(2030, 6, 4, 9, 0, 0), new DateTime(2030, 6, 4, 10, 0, 0), RouteStatus.Cancelled);
            var routeOutsidePeriod = BusinessLogicTestHost.CreateRoute(firstAuto, idleDriver,
                new DateTime(2030, 7, 10, 8, 0, 0), new DateTime(2030, 7, 10, 20, 0, 0), RouteStatus.Completed);
            await _testHost.SeedAsync(firstAuto, secondAuto, busyDriver, idleDriver,
                firstCompletedRoute, secondCompletedRoute, plannedRoute, cancelledRoute, routeOutsidePeriod);

            var periodReport = await _testHost.RunInNewScopeAsync<IRouteService, GetRoutesPeriodReportResponseModel>(
                routeService => routeService.GetRoutesPeriodReportAsync(new GetRoutesPeriodReportRequestModel(
                    PeriodStartDate: new DateOnly(2030, 6, 1),
                    PeriodEndDate: new DateOnly(2030, 6, 30))));

            Assert.Equal(4, periodReport.TotalRoutesCount);
            Assert.Equal(1, periodReport.PlannedRoutesCount);
            Assert.Equal(0, periodReport.InProgressRoutesCount);
            Assert.Equal(2, periodReport.CompletedRoutesCount);
            Assert.Equal(1, periodReport.CancelledRoutesCount);
            Assert.Equal(6.5, periodReport.CompletedRoutesDrivingHours);
            Assert.Equal(2, periodReport.InvolvedAutosCount);
            Assert.Equal(2, periodReport.InvolvedDriversCount);
            Assert.Equal(
                new[] { firstCompletedRoute.Id, secondCompletedRoute.Id, plannedRoute.Id, cancelledRoute.Id },
                periodReport.Routes.Select(route => route.Id));

            var firstPeriodRoute = periodReport.Routes[0];
            Assert.Equal("Kyiv", firstPeriodRoute.StartLocationCity);
            Assert.Equal("Ukraine", firstPeriodRoute.StartLocationCountry);
            Assert.Equal("Lviv", firstPeriodRoute.DestinationLocationCity);
            Assert.Equal(firstAuto.Mark, firstPeriodRoute.AutoMark);
            Assert.Equal(firstAuto.Number, firstPeriodRoute.AutoNumber);
            Assert.Equal(busyDriver.Surname, firstPeriodRoute.DriverSurname);

            var busyDriverSummary = periodReport.DriverSummaries[0];
            Assert.Equal(busyDriver.Id, busyDriverSummary.DriverId);
            Assert.Equal(busyDriver.Name, busyDriverSummary.DriverName);
            Assert.Equal(busyDriver.Surname, busyDriverSummary.DriverSurname);
            Assert.Equal(3, busyDriverSummary.RoutesCount);
            Assert.Equal(2, busyDriverSummary.CompletedRoutesCount);
            Assert.Equal(6.5, busyDriverSummary.CompletedRoutesDrivingHours);

            var idleDriverSummary = periodReport.DriverSummaries[1];
            Assert.Equal(idleDriver.Id, idleDriverSummary.DriverId);
            Assert.Equal(1, idleDriverSummary.RoutesCount);
            Assert.Equal(0, idleDriverSummary.CompletedRoutesCount);
            Assert.Equal(0, idleDriverSummary.CompletedRoutesDrivingHours);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
