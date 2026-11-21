using api_tests.Infrastructure;
using business_logic.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using transport_logistic_management_2026.Services;

namespace api_tests
{
    public sealed class OpenMeteoCityGeocodingClientTests
    {
        private const string TestBaseUrl = "https://geocoding.test/v1/";

        private const string KyivSearchResponseJson = """
            {"results":[
              {"id":703448,"name":"Kyiv","latitude":50.45466,"longitude":30.5238,"country_code":"UA","timezone":"Europe/Kyiv","population":2952301,"country":"Ukraine","admin1":"Kyiv City"},
              {"id":111,"name":"Kyiv Bay","latitude":1.0,"longitude":2.0}
            ],"generationtime_ms":0.1}
            """;

        [Fact]
        public async Task SearchCitiesAsync_ResponseWithIncompleteResult_ReturnsOnlyResultsUsableAsLocations()
        {
            var stubHttpMessageHandler = new StubHttpMessageHandler(HttpStatusCode.OK, KyivSearchResponseJson);

            var cityGeocodingCandidates = await CreateClient(stubHttpMessageHandler).SearchCitiesAsync("Київ", "uk", 5);

            var kyivCandidate = Assert.Single(cityGeocodingCandidates);
            Assert.Equal(703448, kyivCandidate.GeoNamesId);
            Assert.Equal("Kyiv City", kyivCandidate.Region);
            Assert.Equal("Europe/Kyiv", kyivCandidate.TimeZoneId);
            Assert.Equal(2952301, kyivCandidate.Population);
            Assert.Equal(
                "https://geocoding.test/v1/search?name=%D0%9A%D0%B8%D1%97%D0%B2&count=5&language=uk&format=json",
                Assert.Single(stubHttpMessageHandler.RequestedUris).AbsoluteUri);
        }

        [Fact]
        public async Task SearchCitiesAsync_ResponseWithoutResults_ReturnsEmptyList()
        {
            var stubHttpMessageHandler = new StubHttpMessageHandler(HttpStatusCode.OK, """{"generationtime_ms":0.05}""");

            var cityGeocodingCandidates = await CreateClient(stubHttpMessageHandler).SearchCitiesAsync("zzzzqqq", "en", 5);

            Assert.Empty(cityGeocodingCandidates);
        }

        [Fact]
        public async Task FindCityByGeoNamesIdAsync_ServiceAnswersBadRequestForUnknownId_ReturnsNull()
        {
            var stubHttpMessageHandler = new StubHttpMessageHandler(HttpStatusCode.BadRequest, """{"reason":"Location ID not found.","error":true}""");

            var cityGeocodingCandidate = await CreateClient(stubHttpMessageHandler).FindCityByGeoNamesIdAsync(999999999, "en");

            Assert.Null(cityGeocodingCandidate);
        }

        [Fact]
        public async Task SearchCitiesAsync_ServiceAnswersServerError_ThrowsExternalServiceUnavailableException()
        {
            var stubHttpMessageHandler = new StubHttpMessageHandler(HttpStatusCode.InternalServerError, "{}");

            await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
                () => CreateClient(stubHttpMessageHandler).SearchCitiesAsync("Kyiv", "en", 5));
        }

        [Fact]
        public async Task SearchCitiesAsync_ServiceAnswersMalformedJson_ThrowsExternalServiceUnavailableException()
        {
            var stubHttpMessageHandler = new StubHttpMessageHandler(HttpStatusCode.OK, "{\"results\": [");

            await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
                () => CreateClient(stubHttpMessageHandler).SearchCitiesAsync("Kyiv", "en", 5));
        }

        private static OpenMeteoCityGeocodingClient CreateClient(StubHttpMessageHandler stubHttpMessageHandler)
        {
            var stubbedHttpClient = new HttpClient(stubHttpMessageHandler) { BaseAddress = new Uri(TestBaseUrl) };
            return new OpenMeteoCityGeocodingClient(stubbedHttpClient, NullLogger<OpenMeteoCityGeocodingClient>.Instance);
        }
    }
}
