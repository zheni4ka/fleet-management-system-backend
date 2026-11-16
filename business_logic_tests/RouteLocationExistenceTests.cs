using business_logic.DTOs;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class RouteLocationExistenceTests : IDisposable
    {
        private const int MissingLocationId = 999;

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task CreateAsync_DestinationLocationMissing_ThrowsNotFoundWithMissingId()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver);

            var routeDeparture = new DateTime(2030, 2, 1, 8, 0, 0);
            var createRouteRequestModel = RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, routeDeparture, routeDeparture.AddHours(5))
                with { DestinationLocationId = MissingLocationId };

            var notFoundException = await Assert.ThrowsAsync<NotFoundException>(() => _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(
                routeService => routeService.CreateAsync(createRouteRequestModel)));

            Assert.Contains($"'{MissingLocationId}'", notFoundException.Message);
            Assert.DoesNotContain($"'{BusinessLogicTestHost.StartLocationId}", notFoundException.Message);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
