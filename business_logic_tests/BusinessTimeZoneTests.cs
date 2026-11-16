using business_logic.DTOs;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using FluentValidation;

namespace business_logic_tests
{
    public sealed class BusinessTimeZoneTests : IDisposable
    {
        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task GetRoutesPeriodReportAsync_RouteDepartingAfterKyivMidnight_BelongsToNextBusinessDay()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var lateEveningKyivRoute = BusinessLogicTestHost.CreateRoute(
                auto, driver, new DateTime(2026, 11, 26, 21, 30, 0), new DateTime(2026, 11, 26, 21, 50, 0));
            var afterMidnightKyivRoute = BusinessLogicTestHost.CreateRoute(
                auto, driver, new DateTime(2026, 11, 26, 22, 30, 0), new DateTime(2026, 11, 26, 23, 50, 0));
            await _testHost.SeedAsync(lateEveningKyivRoute, afterMidnightKyivRoute);

            var periodReport = await _testHost.RunInNewScopeAsync<IRouteService, GetRoutesPeriodReportResponseModel>(
                routeService => routeService.GetRoutesPeriodReportAsync(new GetRoutesPeriodReportRequestModel(
                    PeriodStartDate: new DateOnly(2026, 11, 26),
                    PeriodEndDate: new DateOnly(2026, 11, 26))));

            Assert.Equal([lateEveningKyivRoute.Id], periodReport.Routes.Select(route => route.Id));
        }

        [Fact]
        public async Task CreateAsync_DepartureTimeWithoutUtcKind_ThrowsValidationException()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver);
            var routeRequestModel = RouteRequestModelFactory.CreateRouteRequestModelFor(
                auto.Id, driver.Id, DateTime.Today.AddDays(2), DateTime.Today.AddDays(3)) with
            {
                DepartureTimeUtc = DateTime.SpecifyKind(DateTime.Today.AddDays(2), DateTimeKind.Unspecified)
            };

            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(
                routeService => routeService.CreateAsync(routeRequestModel)));
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
