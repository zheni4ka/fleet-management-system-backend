using api_tests.Infrastructure;
using business_logic.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using transport_logistic_management_2026.Services;

namespace api_tests
{
    public sealed class PhotonAddressGeocodingClientTests
    {
        private const string TestBaseUrl = "https://photon.test/";

        private const string LvivAddressSearchResponseJson = """
            {"type":"FeatureCollection","features":[
              {"type":"Feature","geometry":{"type":"Point","coordinates":[23.9924747,49.8352691]},
               "properties":{"type":"house","housenumber":"120","street":"Городоцька вулиця","district":"Францівка","city":"Львів","postcode":"79015","countrycode":"UA"}},
              {"type":"Feature","geometry":{"type":"Point","coordinates":[23.9926291,49.8351195]},
               "properties":{"type":"house","housenumber":"120","name":"Нова Пошта №11","street":"Городоцька вулиця","city":"Львів","postcode":"79015"}},
              {"type":"Feature","geometry":{"type":"Point","coordinates":[24.01,49.84]},
               "properties":{"type":"street","name":"Городоцька вулиця","city":"Львів"}},
              {"type":"Feature","geometry":{"type":"Point","coordinates":[24.02,49.85]},
               "properties":{"type":"house","name":"Без номера","city":"Львів"}},
              {"type":"Feature","geometry":{"type":"Point","coordinates":[24.03,49.86]},
               "properties":{"type":"city","name":"Львів"}}
            ]}
            """;

        [Fact]
        public async Task SearchAddressesAsync_HousesStreetsAndOtherResults_ReturnsDistinctAddressesWithLatitudeLongitude()
        {
            var stubHttpMessageHandler = new StubHttpMessageHandler(HttpStatusCode.OK, LvivAddressSearchResponseJson);

            var addressCandidates = await CreateClient(stubHttpMessageHandler).SearchAddressesAsync("Городоцька 120", 49.8397, 24.0297, "uk", 8);

            Assert.Equal(["Городоцька вулиця, 120", "Городоцька вулиця"], addressCandidates.Select(addressCandidate => addressCandidate.AddressLine));
            var houseCandidate = addressCandidates[0];
            Assert.Equal(49.8352691, houseCandidate.Latitude);
            Assert.Equal(23.9924747, houseCandidate.Longitude);
            Assert.Equal("Францівка", houseCandidate.District);
            Assert.Equal("79015", houseCandidate.Postcode);
        }

        [Theory]
        [InlineData("uk", "default")]
        [InlineData("en", "en")]
        public async Task SearchAddressesAsync_UiLanguage_SearchesInsideCityAreaInSupportedLanguage(string uiLanguageCode, string expectedPhotonLanguageCode)
        {
            var stubHttpMessageHandler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"type":"FeatureCollection","features":[]}""");

            await CreateClient(stubHttpMessageHandler).SearchAddressesAsync("Городоцька", 49.8397, 24.0297, uiLanguageCode, 8);

            var requestedQuery = Uri.UnescapeDataString(Assert.Single(stubHttpMessageHandler.RequestedUris).Query);
            Assert.Contains("q=Городоцька", requestedQuery);
            Assert.Contains("lat=49.8397&lon=24.0297", requestedQuery);
            Assert.Contains("bbox=23.6797,49.5897,24.3797,50.0897", requestedQuery);
            Assert.Contains("layer=house&layer=street", requestedQuery);
            Assert.Contains($"lang={expectedPhotonLanguageCode}", requestedQuery);
        }

        [Theory]
        [InlineData(HttpStatusCode.ServiceUnavailable, "{}")]
        [InlineData(HttpStatusCode.OK, "{\"features\": [")]
        public async Task SearchAddressesAsync_ServiceFailsOrAnswersMalformedJson_ThrowsExternalServiceUnavailableException(HttpStatusCode responseStatusCode, string responseJson)
        {
            var stubHttpMessageHandler = new StubHttpMessageHandler(responseStatusCode, responseJson);

            await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
                () => CreateClient(stubHttpMessageHandler).SearchAddressesAsync("Городоцька", 49.8397, 24.0297, "uk", 8));
        }

        private static PhotonAddressGeocodingClient CreateClient(StubHttpMessageHandler stubHttpMessageHandler)
        {
            var stubbedHttpClient = new HttpClient(stubHttpMessageHandler) { BaseAddress = new Uri(TestBaseUrl) };
            return new PhotonAddressGeocodingClient(stubbedHttpClient, NullLogger<PhotonAddressGeocodingClient>.Instance);
        }
    }
}
