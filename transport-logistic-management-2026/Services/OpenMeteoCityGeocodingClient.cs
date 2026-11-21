using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Exceptions;
using business_logic.Interfaces;
using System.Globalization;
using System.Net;
using System.Text.Json;
using transport_logistic_management_2026.ExternalModels;

namespace transport_logistic_management_2026.Services
{
    public class OpenMeteoCityGeocodingClient : ICityGeocodingClient
    {
        private const string SearchPathTemplate = "search?name={0}&count={1}&language={2}&format=json";
        private const string GetByIdPathTemplate = "get?id={0}&language={1}";

        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenMeteoCityGeocodingClient> _logger;

        public OpenMeteoCityGeocodingClient(HttpClient httpClient, ILogger<OpenMeteoCityGeocodingClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IReadOnlyList<GetCityGeocodingCandidateResponseModel>> SearchCitiesAsync(string searchText, string languageCode, int resultsCount)
        {
            var searchPath = string.Format(
                CultureInfo.InvariantCulture,
                SearchPathTemplate,
                Uri.EscapeDataString(searchText),
                resultsCount,
                Uri.EscapeDataString(languageCode));
            var searchResponse = await SendGeocodingRequestAsync<OpenMeteoGeocodingSearchResponse>(searchPath, isBadRequestNotFound: false);
            if (searchResponse?.Results is null)
            {
                return [];
            }

            return searchResponse.Results
                .Where(IsGeocodingResultUsableAsLocation)
                .Select(MapToCityGeocodingCandidate)
                .ToList();
        }

        public async Task<GetCityGeocodingCandidateResponseModel?> FindCityByGeoNamesIdAsync(int geoNamesId, string languageCode)
        {
            var getByIdPath = string.Format(
                CultureInfo.InvariantCulture,
                GetByIdPathTemplate,
                geoNamesId,
                Uri.EscapeDataString(languageCode));
            var geocodingResult = await SendGeocodingRequestAsync<OpenMeteoGeocodingResult>(getByIdPath, isBadRequestNotFound: true);
            if (geocodingResult is null || !IsGeocodingResultUsableAsLocation(geocodingResult))
            {
                return null;
            }

            return MapToCityGeocodingCandidate(geocodingResult);
        }

        private async Task<TGeocodingResponse?> SendGeocodingRequestAsync<TGeocodingResponse>(string requestPath, bool isBadRequestNotFound)
            where TGeocodingResponse : class
        {
            try
            {
                using var geocodingResponse = await _httpClient.GetAsync(requestPath);
                if (isBadRequestNotFound && geocodingResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }

                geocodingResponse.EnsureSuccessStatusCode();
                return await geocodingResponse.Content.ReadFromJsonAsync<TGeocodingResponse>();
            }
            catch (Exception geocodingException) when (geocodingException is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(geocodingException, "City geocoding request '{RequestPath}' failed.", requestPath);
                throw new ExternalServiceUnavailableException(ExceptionMessages.CityGeocodingServiceUnavailable, geocodingException);
            }
        }

        private static bool IsGeocodingResultUsableAsLocation(OpenMeteoGeocodingResult geocodingResult)
        {
            return !string.IsNullOrWhiteSpace(geocodingResult.Country)
                && !string.IsNullOrWhiteSpace(geocodingResult.CountryCode)
                && !string.IsNullOrWhiteSpace(geocodingResult.TimeZoneId);
        }

        private static GetCityGeocodingCandidateResponseModel MapToCityGeocodingCandidate(OpenMeteoGeocodingResult geocodingResult)
        {
            return new GetCityGeocodingCandidateResponseModel
            {
                GeoNamesId = geocodingResult.Id,
                City = geocodingResult.Name,
                Region = geocodingResult.FirstLevelAdministrativeRegion,
                Country = geocodingResult.Country!,
                CountryCode = geocodingResult.CountryCode!,
                Latitude = geocodingResult.Latitude,
                Longitude = geocodingResult.Longitude,
                TimeZoneId = geocodingResult.TimeZoneId!,
                Population = geocodingResult.Population
            };
        }
    }
}
