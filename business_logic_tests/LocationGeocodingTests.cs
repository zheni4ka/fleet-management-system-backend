using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using data_access.data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace business_logic_tests
{
    public sealed class LocationGeocodingTests : IDisposable
    {
        private const int KyivGeoNamesId = 703448;
        private const int KyivVillageGeoNamesId = 704213;
        private const int LvivPolishNamesakeGeoNamesId = 900001;
        private const int SeededKyivLocationId = 1;
        private const int SeededLvivLocationId = 2;

        private static readonly GetCityGeocodingCandidateResponseModel KyivCityCandidate = CreateCityGeocodingCandidate(
            KyivGeoNamesId, "Kyiv", "Kyiv City", LocationConstraints.UkraineCountryName, 50.45466, 30.5238, LocationConstraints.UkraineTimeZoneId, 2952301);

        private static readonly GetCityGeocodingCandidateResponseModel KyivVillageCandidate = CreateCityGeocodingCandidate(
            KyivVillageGeoNamesId, "Kyiv", "Mykolaiv Oblast", LocationConstraints.UkraineCountryName, 47.86401, 31.02245, LocationConstraints.UkraineTimeZoneId, 108);

        private const int BilaTserkvaGeoNamesId = 711660;

        private static readonly GetCityGeocodingCandidateResponseModel BilaTserkvaCityCandidate = CreateCityGeocodingCandidate(
            BilaTserkvaGeoNamesId, "Bila Tserkva", "Kyiv Oblast", LocationConstraints.UkraineCountryName, 49.80939, 30.11209, LocationConstraints.UkraineTimeZoneId, 211080);

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task EnsureCityLocationAsync_CityWithoutSavedLocation_CreatesLocationFromGeocodingService()
        {
            SetSearchResults("Bila Tserkva", BilaTserkvaCityCandidate);

            var createdLocation = await EnsureCityLocationAsync(BilaTserkvaGeoNamesId);

            Assert.Equal("Bila Tserkva", createdLocation.City);
            Assert.Equal(LocationConstraints.UkraineCountryName, createdLocation.Country);
            Assert.Equal(BilaTserkvaGeoNamesId, createdLocation.GeoNamesId);
            Assert.Equal(49.80939, createdLocation.Latitude);
            Assert.Equal(30.11209, createdLocation.Longitude);
            Assert.Equal(LocationConstraints.UkraineTimeZoneId, createdLocation.TimeZoneId);
            Assert.Equal([SupportedLanguages.English], GetTestCityGeocodingClient().RequestedLanguageCodes);
            Assert.Equal(1, await CountAuditLogsAsync(AuditAction.LocationCreated));
        }

        [Fact]
        public async Task EnsureCityLocationAsync_SavedLocationWithSameCityAndCountryWithoutGeoNamesId_FillsItsCoordinatesInsteadOfCreatingDuplicate()
        {
            SetSearchResults("Kyiv", KyivCityCandidate);

            var ensuredLocation = await EnsureCityLocationAsync(KyivGeoNamesId);

            Assert.Equal(SeededKyivLocationId, ensuredLocation.Id);
            var persistedKyivLocation = await GetPersistedLocationAsync(SeededKyivLocationId);
            Assert.Equal(KyivGeoNamesId, persistedKyivLocation.GeoNamesId);
            Assert.Equal(50.45466, persistedKyivLocation.Latitude);
            Assert.Equal(1, await CountAuditLogsAsync(AuditAction.LocationCoordinatesFilled));
            Assert.Equal(0, await CountAuditLogsAsync(AuditAction.LocationCreated));
        }

        [Fact]
        public async Task EnsureCityLocationAsync_SameCityPickedTwice_ReturnsSavedLocationWithoutCallingGeocodingServiceAgain()
        {
            SetSearchResults("Bila Tserkva", BilaTserkvaCityCandidate);

            var firstEnsuredLocation = await EnsureCityLocationAsync(BilaTserkvaGeoNamesId);
            var secondEnsuredLocation = await EnsureCityLocationAsync(BilaTserkvaGeoNamesId);

            Assert.Equal(firstEnsuredLocation.Id, secondEnsuredLocation.Id);
            Assert.Single(GetTestCityGeocodingClient().RequestedLanguageCodes);
            Assert.Equal(1, await CountAuditLogsAsync(AuditAction.LocationCreated));
        }

        [Fact]
        public async Task EnsureCityLocationAsync_ArchivedLocationOfPickedCity_RestoresIt()
        {
            SetSearchResults("Bila Tserkva", BilaTserkvaCityCandidate);
            var ensuredLocation = await EnsureCityLocationAsync(BilaTserkvaGeoNamesId);
            await _testHost.RunInNewScopeAsync<FMS_DbContext>(async context =>
            {
                var savedLocation = await context.Set<Location>().SingleAsync(location => location.Id == ensuredLocation.Id);
                savedLocation.IsArchived = true;
                await context.SaveChangesAsync();
            });

            var restoredLocation = await EnsureCityLocationAsync(BilaTserkvaGeoNamesId);

            Assert.Equal(ensuredLocation.Id, restoredLocation.Id);
            Assert.False(restoredLocation.IsArchived);
            Assert.Equal(1, await CountAuditLogsAsync(AuditAction.LocationRestored));
        }

        [Fact]
        public async Task EnsureCityLocationAsync_GeoNamesIdUnknownToGeocodingService_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(() => EnsureCityLocationAsync(KyivGeoNamesId));
        }

        [Fact]
        public async Task SearchCityGeocodingCandidatesAsync_SearchTextShorterThanMinimum_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<ILocationService, IReadOnlyList<GetCityGeocodingCandidateResponseModel>>(
                locationService => locationService.SearchCityGeocodingCandidatesAsync(new GetCityGeocodingCandidatesRequestModel(SearchText: "K"))));
        }

        [Fact]
        public async Task SearchCityGeocodingCandidatesAsync_ValidSearchText_SearchesInCurrentUiLanguage()
        {
            SetSearchResults("Kyiv", KyivCityCandidate, KyivVillageCandidate);

            var cityGeocodingCandidates = await _testHost.RunInNewScopeAsync<ILocationService, IReadOnlyList<GetCityGeocodingCandidateResponseModel>>(
                locationService => locationService.SearchCityGeocodingCandidatesAsync(new GetCityGeocodingCandidatesRequestModel(SearchText: " Kyiv ")));

            Assert.Equal([KyivGeoNamesId, KyivVillageGeoNamesId], cityGeocodingCandidates.Select(cityGeocodingCandidate => cityGeocodingCandidate.GeoNamesId));
            Assert.Equal([SupportedLanguages.Default], GetTestCityGeocodingClient().RequestedLanguageCodes);
        }

        [Fact]
        public async Task SearchCityGeocodingCandidatesAsync_GeocodingServiceUnavailable_ThrowsExternalServiceUnavailableException()
        {
            GetTestCityGeocodingClient().IsServiceUnavailable = true;

            await Assert.ThrowsAsync<ExternalServiceUnavailableException>(() => _testHost.RunInNewScopeAsync<ILocationService, IReadOnlyList<GetCityGeocodingCandidateResponseModel>>(
                locationService => locationService.SearchCityGeocodingCandidatesAsync(new GetCityGeocodingCandidatesRequestModel(SearchText: "Kyiv"))));
        }

        [Fact]
        public async Task FillMissingCoordinatesAsync_SeveralCandidatesInCountry_FillsMostPopulousAndReportsOtherCandidates()
        {
            SetSearchResults("Kyiv", KyivVillageCandidate, KyivCityCandidate);

            var fillResult = await FillMissingCoordinatesAsync();

            var filledKyiv = Assert.Single(fillResult.FilledLocations);
            Assert.Equal(SeededKyivLocationId, filledKyiv.LocationId);
            Assert.Equal(KyivGeoNamesId, filledKyiv.GeoNamesId);
            Assert.Equal(1, filledKyiv.OtherCandidatesInCountryCount);
            var persistedKyiv = await GetPersistedLocationAsync(SeededKyivLocationId);
            Assert.Equal(50.45466, persistedKyiv.Latitude);
            Assert.Equal(30.5238, persistedKyiv.Longitude);
            Assert.Equal(KyivGeoNamesId, persistedKyiv.GeoNamesId);
            Assert.Equal(1, await CountAuditLogsAsync(AuditAction.LocationCoordinatesFilled));
        }

        [Fact]
        public async Task FillMissingCoordinatesAsync_CandidatesOnlyInOtherCountry_ReportsLocationAsUnresolvedWithoutChangingIt()
        {
            SetSearchResults("Lviv", CreateCityGeocodingCandidate(
                LvivPolishNamesakeGeoNamesId, "Lviv", null, LocationConstraints.PolandCountryName, 50.0, 20.0, LocationConstraints.PolandTimeZoneId, 50));

            var fillResult = await FillMissingCoordinatesAsync();

            var unresolvedLviv = Assert.Single(fillResult.UnresolvedLocations, unresolvedLocation => unresolvedLocation.LocationId == SeededLvivLocationId);
            Assert.Equal(UnresolvedLocationCoordinatesReason.NoCandidateInCountry, unresolvedLviv.Reason);
            var persistedLviv = await GetPersistedLocationAsync(SeededLvivLocationId);
            Assert.Null(persistedLviv.Latitude);
            Assert.Null(persistedLviv.GeoNamesId);
        }

        [Fact]
        public async Task FillMissingCoordinatesAsync_TwoLocationsMatchSameCity_FillsFirstAndReportsSecondAsDuplicate()
        {
            SetSearchResults("Kyiv", KyivCityCandidate);
            SetSearchResults("Kiev", KyivCityCandidate);
            var duplicateKyivLocation = new Location { City = "Kiev", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId };
            await _testHost.SeedAsync(duplicateKyivLocation);

            var fillResult = await FillMissingCoordinatesAsync();

            Assert.Equal(SeededKyivLocationId, Assert.Single(fillResult.FilledLocations).LocationId);
            var unresolvedDuplicate = Assert.Single(fillResult.UnresolvedLocations, unresolvedLocation => unresolvedLocation.LocationId == duplicateKyivLocation.Id);
            Assert.Equal(UnresolvedLocationCoordinatesReason.GeoNamesIdUsedByAnotherLocation, unresolvedDuplicate.Reason);
        }

        [Fact]
        public async Task FillMissingCoordinatesAsync_EveryLocationWithoutCoordinates_IsEitherFilledOrUnresolved()
        {
            SetSearchResults("Kyiv", KyivCityCandidate);
            var locationsWithoutCoordinatesCount = await _testHost.RunInNewScopeAsync<FMS_DbContext, int>(
                context => context.Set<Location>().CountAsync(location => location.Latitude == null));

            var fillResult = await FillMissingCoordinatesAsync();

            Assert.Equal(locationsWithoutCoordinatesCount, fillResult.FilledLocations.Count + fillResult.UnresolvedLocations.Count);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }

        private static GetCityGeocodingCandidateResponseModel CreateCityGeocodingCandidate(
            int geoNamesId, string city, string? region, string country, double latitude, double longitude, string timeZoneId, int population)
        {
            return new GetCityGeocodingCandidateResponseModel
            {
                GeoNamesId = geoNamesId,
                City = city,
                Region = region,
                Country = country,
                CountryCode = country == LocationConstraints.UkraineCountryName ? "UA" : "PL",
                Latitude = latitude,
                Longitude = longitude,
                TimeZoneId = timeZoneId,
                Population = population
            };
        }

        private TestCityGeocodingClient GetTestCityGeocodingClient()
        {
            return _testHost.GetRequiredSingletonService<TestCityGeocodingClient>();
        }

        private void SetSearchResults(string searchText, params GetCityGeocodingCandidateResponseModel[] cityGeocodingCandidates)
        {
            GetTestCityGeocodingClient().SetSearchResults(searchText, cityGeocodingCandidates);
        }

        private Task<GetLocationResponseModel> EnsureCityLocationAsync(int geoNamesId)
        {
            return _testHost.RunInNewScopeAsync<ILocationService, GetLocationResponseModel>(
                locationService => locationService.EnsureCityLocationAsync(new CreateLocationRequestModel { GeoNamesId = geoNamesId }));
        }

        private Task<FillMissingLocationCoordinatesResponseModel> FillMissingCoordinatesAsync()
        {
            return _testHost.RunInNewScopeAsync<ILocationService, FillMissingLocationCoordinatesResponseModel>(
                locationService => locationService.FillMissingCoordinatesAsync());
        }

        private Task<Location> GetPersistedLocationAsync(int locationId)
        {
            return _testHost.RunInNewScopeAsync<FMS_DbContext, Location>(
                context => context.Set<Location>().AsNoTracking().SingleAsync(location => location.Id == locationId));
        }

        private Task<int> CountAuditLogsAsync(AuditAction auditAction)
        {
            return _testHost.RunInNewScopeAsync<FMS_DbContext, int>(
                context => context.Set<AuditLog>().CountAsync(auditLog => auditLog.Action == auditAction));
        }
    }
}
