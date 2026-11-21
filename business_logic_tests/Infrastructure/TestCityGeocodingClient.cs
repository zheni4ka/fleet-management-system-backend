using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Exceptions;
using business_logic.Interfaces;

namespace business_logic_tests.Infrastructure
{
    public sealed class TestCityGeocodingClient : ICityGeocodingClient
    {
        private readonly Dictionary<string, IReadOnlyList<GetCityGeocodingCandidateResponseModel>> _searchResultsBySearchText = new(StringComparer.OrdinalIgnoreCase);

        public bool IsServiceUnavailable { get; set; }

        public List<string> RequestedLanguageCodes { get; } = [];

        public void SetSearchResults(string searchText, params GetCityGeocodingCandidateResponseModel[] cityGeocodingCandidates)
        {
            _searchResultsBySearchText[searchText] = cityGeocodingCandidates;
        }

        public Task<IReadOnlyList<GetCityGeocodingCandidateResponseModel>> SearchCitiesAsync(string searchText, string languageCode, int resultsCount)
        {
            ThrowWhenServiceUnavailable();
            RequestedLanguageCodes.Add(languageCode);
            if (!_searchResultsBySearchText.TryGetValue(searchText, out var cityGeocodingCandidates))
            {
                return Task.FromResult<IReadOnlyList<GetCityGeocodingCandidateResponseModel>>([]);
            }

            return Task.FromResult<IReadOnlyList<GetCityGeocodingCandidateResponseModel>>(cityGeocodingCandidates.Take(resultsCount).ToList());
        }

        public Task<GetCityGeocodingCandidateResponseModel?> FindCityByGeoNamesIdAsync(int geoNamesId, string languageCode)
        {
            ThrowWhenServiceUnavailable();
            RequestedLanguageCodes.Add(languageCode);
            var matchingCityGeocodingCandidate = _searchResultsBySearchText.Values
                .SelectMany(cityGeocodingCandidates => cityGeocodingCandidates)
                .FirstOrDefault(cityGeocodingCandidate => cityGeocodingCandidate.GeoNamesId == geoNamesId);
            return Task.FromResult(matchingCityGeocodingCandidate);
        }

        private void ThrowWhenServiceUnavailable()
        {
            if (IsServiceUnavailable)
            {
                throw new ExternalServiceUnavailableException(
                    ExceptionMessages.CityGeocodingServiceUnavailable,
                    new HttpRequestException("Test city geocoding service is unavailable."));
            }
        }
    }
}
