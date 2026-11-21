using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface ICityGeocodingClient
    {
        Task<IReadOnlyList<GetCityGeocodingCandidateResponseModel>> SearchCitiesAsync(string searchText, string languageCode, int resultsCount);
        Task<GetCityGeocodingCandidateResponseModel?> FindCityByGeoNamesIdAsync(int geoNamesId, string languageCode);
    }
}
