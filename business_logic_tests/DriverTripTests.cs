using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Helpers;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using data_access.data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace business_logic_tests
{
    public sealed class DriverTripTests : IDisposable
    {
        private static readonly DateTime RouteDeparture = new(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime RouteArrival = new(2030, 6, 1, 18, 0, 0, DateTimeKind.Utc);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task UpdateAsync_InProgressRouteCompleted_RecordsTripSnapshot()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var actualDepartureUtc = DateTime.UtcNow.AddHours(-3);
            var route = CreateStartedRoute(auto, driver, actualDepartureUtc);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Completed)));

            var driverTrip = Assert.Single(await ReadDriverTripsAsync());
            Assert.Equal(route.Id, driverTrip.RouteId);
            Assert.Equal(driver.Id, driverTrip.DriverId);
            Assert.Equal(auto.Number, driverTrip.AutoNumber);
            Assert.Equal("Kyiv", driverTrip.StartLocationCity);
            Assert.Equal("Lviv", driverTrip.DestinationLocationCity);
            Assert.Equal(actualDepartureUtc, driverTrip.StartedAtUtc);
            Assert.Equal(RouteStatus.Completed, driverTrip.FinalStatus);
            Assert.InRange(driverTrip.DrivingMinutes, 179, 181);
        }

        [Fact]
        public async Task UpdateAsync_InProgressRouteCancelled_RecordsCancelledTrip()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = CreateStartedRoute(auto, driver, RouteDeparture);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Cancelled)));

            Assert.Equal(RouteStatus.Cancelled, Assert.Single(await ReadDriverTripsAsync()).FinalStatus);
        }

        [Fact]
        public async Task UpdateAsync_PlannedRouteCancelled_RecordsNoTrip()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival);
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Cancelled)));

            Assert.Empty(await ReadDriverTripsAsync());
        }

        [Fact]
        public async Task MaintenanceCreateAsync_InProgressRouteCancelled_RecordsTrip()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var departureUtc = DateTime.UtcNow.AddHours(-2);
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, departureUtc, DateTime.UtcNow.AddDays(3), RouteStatus.InProgress);
            route.ActualDepartureTimeUtc = departureUtc;
            await _testHost.SeedAsync(auto, driver, route);

            await _testHost.RunInNewScopeAsync<IAutoMaintenanceService, GetAutoMaintenanceResponseModel>(maintenanceService => maintenanceService.CreateAsync(
                new CreateAutoMaintenanceRequestModel
                {
                    AutoId = auto.Id,
                    Name = "Ремонт",
                    Description = "Аварійна зупинка",
                    ServiceDate = BusinessTimeZoneConverter.GetBusinessNowDateTime().AddDays(1)
                }));

            Assert.Equal(RouteStatus.Cancelled, Assert.Single(await ReadDriverTripsAsync()).FinalStatus);
        }

        [Fact]
        public async Task RouteDeleteAsync_CompletedRouteWithTrip_KeepsTripWithoutRoute()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", AutoStatus.InService);
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var route = CreateStartedRoute(auto, driver, RouteDeparture);
            await _testHost.SeedAsync(auto, driver, route);
            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.UpdateAsync(
                RouteRequestModelFactory.CreateUpdateRouteRequestModelFor(route, auto.Id, driver.Id, RouteStatus.Completed)));

            await _testHost.RunInNewScopeAsync<IRouteService>(routeService => routeService.DeleteAsync(route.Id));

            var driverTrip = Assert.Single(await ReadDriverTripsAsync());
            Assert.Null(driverTrip.RouteId);

            var deleteDriverResponse = await _testHost.RunInNewScopeAsync<IDriverService, DeleteDriverResponseModel>(
                driverService => driverService.DeleteAsync(driver.Id));
            Assert.True(deleteDriverResponse.IsArchivedInsteadOfDeleted);
        }

        [Fact]
        public async Task GetSummaryAndPagedListAsync_SeveralTrips_AggregatesAndOrdersNewestFirst()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(auto, driver);
            await _testHost.SeedAsync(
                CreateDriverTrip(driver, auto, RouteDeparture, 120, RouteStatus.Completed),
                CreateDriverTrip(driver, auto, RouteDeparture.AddDays(2), 90, RouteStatus.Completed),
                CreateDriverTrip(driver, auto, RouteDeparture.AddDays(1), 30, RouteStatus.Cancelled));

            var driverTripSummary = await _testHost.RunInNewScopeAsync<IDriverTripService, GetDriverTripSummaryResponseModel>(
                driverTripService => driverTripService.GetSummaryAsync(driver.Id));
            var driverTripsPage = await _testHost.RunInNewScopeAsync<IDriverTripService, GetPagedListResponseModel<GetDriverTripResponseModel>>(
                driverTripService => driverTripService.GetPagedListAsync(
                    new GetDriverTripsRequestModel(driver.Id),
                    new GetPagedListRequestModel<DriverTripSortField>(Page: 1, PageSize: 2, Sort: null, SortDirection: null, Search: null)));

            Assert.Equal(3, driverTripSummary.TripsCount);
            Assert.Equal(2, driverTripSummary.CompletedTripsCount);
            Assert.Equal(1, driverTripSummary.CancelledTripsCount);
            Assert.Equal(240, driverTripSummary.DrivingMinutes);
            Assert.Equal(RouteDeparture.AddDays(2).AddMinutes(90), driverTripSummary.LastTripEndedAtUtc);
            Assert.Equal(3, driverTripsPage.TotalCount);
            Assert.Equal(
                new[] { RouteDeparture.AddDays(2), RouteDeparture.AddDays(1) },
                driverTripsPage.Items.Select(driverTrip => driverTrip.StartedAtUtc));
        }

        [Fact]
        public async Task DriverCreateAsync_ContactsWithSpaces_StoresTrimmedValuesAndNullForBlank()
        {
            var createdDriver = await _testHost.RunInNewScopeAsync<IDriverService, GetDriverResponseModel>(driverService => driverService.CreateAsync(
                new CreateDriverRequestModel
                {
                    Name = "Тарас",
                    Surname = "Шевченко",
                    Patronymic = "Григорович",
                    LicenseNumber = "ABC 123456",
                    PhoneNumber = " +380671234567 ",
                    Email = "  ",
                    DepotId = 1
                }));

            Assert.Equal("+380671234567", createdDriver.PhoneNumber);
            Assert.Null(createdDriver.Email);
        }

        [Theory]
        [InlineData("0671234567", null)]
        [InlineData("+380671234567", "not-an-email")]
        public async Task DriverCreateAsync_InvalidContacts_ThrowsValidation(string phoneNumber, string? email)
        {
            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<IDriverService, GetDriverResponseModel>(driverService => driverService.CreateAsync(
                new CreateDriverRequestModel
                {
                    Name = "Тарас",
                    Surname = "Шевченко",
                    Patronymic = "Григорович",
                    LicenseNumber = "ABC 123456",
                    PhoneNumber = phoneNumber,
                    Email = email,
                    DepotId = 1
                })));
        }

        private static Route CreateStartedRoute(Auto auto, Driver driver, DateTime actualDepartureUtc)
        {
            var route = BusinessLogicTestHost.CreateRoute(auto, driver, RouteDeparture, RouteArrival, RouteStatus.InProgress);
            route.ActualDepartureTimeUtc = actualDepartureUtc;
            return route;
        }

        private static DriverTrip CreateDriverTrip(Driver driver, Auto auto, DateTime startedAtUtc, int drivingMinutes, RouteStatus finalStatus)
        {
            return new DriverTrip
            {
                DriverId = driver.Id,
                AutoId = auto.Id,
                AutoMark = auto.Mark,
                AutoModel = auto.Model,
                AutoNumber = auto.Number,
                StartLocationCity = "Kyiv",
                StartLocationCountry = "Ukraine",
                StartLocationTimeZoneId = "Europe/Kyiv",
                DestinationLocationCity = "Lviv",
                DestinationLocationCountry = "Ukraine",
                DestinationLocationTimeZoneId = "Europe/Kyiv",
                PlannedDepartureTimeUtc = startedAtUtc,
                PlannedArrivalTimeUtc = startedAtUtc.AddMinutes(drivingMinutes),
                StartedAtUtc = startedAtUtc,
                EndedAtUtc = startedAtUtc.AddMinutes(drivingMinutes),
                DrivingMinutes = drivingMinutes,
                FinalStatus = finalStatus,
                DepotId = driver.DepotId
            };
        }

        private async Task<IReadOnlyList<DriverTrip>> ReadDriverTripsAsync()
        {
            using var serviceScope = _testHost.Services.CreateScope();
            return await serviceScope.ServiceProvider.GetRequiredService<FMS_DbContext>().Set<DriverTrip>().ToListAsync();
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
