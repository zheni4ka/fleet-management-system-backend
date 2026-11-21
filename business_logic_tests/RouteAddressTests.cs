using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using data_access.data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace business_logic_tests
{
    public sealed class RouteAddressTests : IDisposable
    {
        private static readonly DateTime RouteDeparture = new(2030, 7, 1, 8, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime RouteArrival = new(2030, 7, 1, 18, 0, 0, DateTimeKind.Utc);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task CreateAsync_AddressesWithSpaces_StoresTrimmedAddressesAndCoordinates()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver);

            var createdRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.CreateAsync(
                RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, RouteDeparture, RouteArrival) with
                {
                    StartAddressLine = "  вулиця Хрещатик, 22  "
                }));

            Assert.Equal(RouteRequestModelFactory.KyivStartAddressLine, createdRoute.StartAddressLine);
            Assert.Equal(50.4501, createdRoute.StartAddressLatitude);
            Assert.Equal(RouteRequestModelFactory.LvivDestinationAddressLine, createdRoute.DestinationAddressLine);
            Assert.Equal(23.9924747, createdRoute.DestinationAddressLongitude);
        }

        [Fact]
        public async Task CreateAsync_DestinationAddressMissing_ThrowsValidationWithDestinationAddressError()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver);

            var validationException = await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(
                routeService => routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(auto.Id, driver.Id, RouteDeparture, RouteArrival) with
                {
                    DestinationAddressLine = null,
                    DestinationAddressLatitude = null,
                    DestinationAddressLongitude = null
                })));

            Assert.Contains(validationException.Errors, validationFailure => validationFailure.PropertyName == nameof(CreateRouteRequestModel.DestinationAddressLine));
            Assert.Contains(validationException.Errors, validationFailure => validationFailure.PropertyName == nameof(CreateRouteRequestModel.DestinationAddressLatitude));
        }

        [Fact]
        public async Task UpdateAsync_StatusChangeWithoutAddresses_KeepsStoredAddresses()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = CreateRouteWithAddresses(auto, driver);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Cancelled)));

            var updatedRoute = await GetRouteAsync(route.Id);
            Assert.Equal(RouteStatus.Cancelled, updatedRoute.Status);
            Assert.Equal(RouteRequestModelFactory.KyivStartAddressLine, updatedRoute.StartAddressLine);
            Assert.Equal(RouteRequestModelFactory.LvivDestinationAddressLine, updatedRoute.DestinationAddressLine);
        }

        [Fact]
        public async Task UpdateAsync_NewDestinationAddress_ReplacesOnlyDestination()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = CreateRouteWithAddresses(auto, driver);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Planned) with
                {
                    DestinationAddressLine = "площа Ринок, 1",
                    DestinationAddressLatitude = 49.8419,
                    DestinationAddressLongitude = 24.0315
                }));

            var updatedRoute = await GetRouteAsync(route.Id);
            Assert.Equal(RouteRequestModelFactory.KyivStartAddressLine, updatedRoute.StartAddressLine);
            Assert.Equal("площа Ринок, 1", updatedRoute.DestinationAddressLine);
            Assert.Equal(49.8419, updatedRoute.DestinationAddressLatitude);
        }

        [Fact]
        public async Task UpdateAsync_AddressWithoutCoordinates_ThrowsValidation()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = CreateRouteWithAddresses(auto, driver);
            await _testHost.SeedAsync(auto, driver, route);

            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Planned) with
                {
                    StartAddressLine = "вулиця Хрещатик, 1"
                })));
        }

        [Fact]
        public async Task UpdateAsync_StartedRouteCompleted_SnapshotsAddressesInTrip()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = CreateRouteWithAddresses(auto, driver);
            route.Status = RouteStatus.InProgress;
            route.ActualDepartureTimeUtc = DateTime.UtcNow.AddHours(-2);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Completed)));

            using var serviceScope = _testHost.Services.CreateScope();
            var driverTrip = await serviceScope.ServiceProvider.GetRequiredService<FMS_DbContext>().Set<DriverTrip>().SingleAsync();
            Assert.Equal(RouteRequestModelFactory.KyivStartAddressLine, driverTrip.StartAddressLine);
            Assert.Equal(RouteRequestModelFactory.LvivDestinationAddressLine, driverTrip.DestinationAddressLine);
        }

        [Fact]
        public async Task SearchAddressCandidatesAsync_ValidRequest_SearchesNearCityInUiLanguage()
        {
            var testAddressGeocodingClient = _testHost.GetRequiredSingletonService<TestAddressGeocodingClient>();

            await _testHost.RunInNewScopeAsync<ILocationService, IReadOnlyList<GetAddressCandidateResponseModel>>(locationService => locationService.SearchAddressCandidatesAsync(
                new GetAddressCandidatesRequestModel(SearchText: "  Городоцька 120 ", Latitude: 49.8397, Longitude: 24.0297)));

            var addressSearch = Assert.Single(testAddressGeocodingClient.Searches);
            Assert.Equal("Городоцька 120", addressSearch.SearchText);
            Assert.Equal(49.8397, addressSearch.Latitude);
            Assert.Equal(24.0297, addressSearch.Longitude);
            Assert.Equal(AddressGeocodingConstraints.SearchResultsMaxCount, addressSearch.ResultsCount);
        }

        [Theory]
        [InlineData("Го", 49.8, 24.0)]
        [InlineData("Городоцька", 91.0, 24.0)]
        public async Task SearchAddressCandidatesAsync_InvalidRequest_ThrowsValidationWithoutCallingProvider(string searchText, double latitude, double longitude)
        {
            var testAddressGeocodingClient = _testHost.GetRequiredSingletonService<TestAddressGeocodingClient>();

            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<ILocationService, IReadOnlyList<GetAddressCandidateResponseModel>>(
                locationService => locationService.SearchAddressCandidatesAsync(new GetAddressCandidatesRequestModel(searchText, latitude, longitude))));

            Assert.Empty(testAddressGeocodingClient.Searches);
        }

        private static Route CreateRouteWithAddresses(Auto auto, Driver driver)
        {
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            route.StartAddressLine = RouteRequestModelFactory.KyivStartAddressLine;
            route.StartAddressLatitude = 50.4501;
            route.StartAddressLongitude = 30.5234;
            route.DestinationAddressLine = RouteRequestModelFactory.LvivDestinationAddressLine;
            route.DestinationAddressLatitude = 49.8352691;
            route.DestinationAddressLongitude = 23.9924747;
            return route;
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
