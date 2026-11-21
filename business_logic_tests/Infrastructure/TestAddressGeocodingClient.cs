using business_logic.DTOs;
using business_logic.Interfaces;

namespace business_logic_tests.Infrastructure
{
    public sealed class TestAddressGeocodingClient : IAddressGeocodingClient
    {
        public List<(string SearchText, double Latitude, double Longitude, string LanguageCode, int ResultsCount)> Searches { get; } = [];

        public IReadOnlyList<GetAddressCandidateResponseModel> SearchResults { get; set; } = [];

        public Task<IReadOnlyList<GetAddressCandidateResponseModel>> SearchAddressesAsync(
            string searchText, double latitude, double longitude, string languageCode, int resultsCount)
        {
            Searches.Add((searchText, latitude, longitude, languageCode, resultsCount));
            return Task.FromResult<IReadOnlyList<GetAddressCandidateResponseModel>>(SearchResults.Take(resultsCount).ToList());
        }
    }
}
