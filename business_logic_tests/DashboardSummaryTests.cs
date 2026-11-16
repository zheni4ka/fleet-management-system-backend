using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class DashboardSummaryTests : IDisposable
    {
        private static readonly DateTime FirstRouteDeparture = new(2030, 4, 1, 8, 0, 0);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task GetDashboardSummaryAsync_RoutesInEveryStatus_CountsOnlyPlannedAndInProgressAsActive()
        {
            var firstAuto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var secondAuto = BusinessLogicTestHost.CreateAuto("АА2222АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(firstAuto, secondAuto, driver,
                BusinessLogicTestHost.CreateRoute(firstAuto, driver, FirstRouteDeparture, FirstRouteDeparture.AddHours(2)),
                BusinessLogicTestHost.CreateRoute(firstAuto, driver, FirstRouteDeparture.AddDays(1), FirstRouteDeparture.AddDays(1).AddHours(2), RouteStatus.InProgress),
                BusinessLogicTestHost.CreateRoute(secondAuto, driver, FirstRouteDeparture.AddDays(2), FirstRouteDeparture.AddDays(2).AddHours(2), RouteStatus.Completed),
                BusinessLogicTestHost.CreateRoute(secondAuto, driver, FirstRouteDeparture.AddDays(3), FirstRouteDeparture.AddDays(3).AddHours(2), RouteStatus.Cancelled));

            var dashboardSummary = await _testHost.RunInNewScopeAsync<IDashboardService, GetDashboardSummaryResponseModel>(
                dashboardService => dashboardService.GetDashboardSummaryAsync());

            Assert.Equal(new GetDashboardSummaryResponseModel(
                AutosCount: 2,
                DriversCount: 1,
                RoutesCount: 4,
                ActiveRoutesCount: 2), dashboardSummary);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
