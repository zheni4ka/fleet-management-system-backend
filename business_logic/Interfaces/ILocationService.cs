using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface ILocationService
    {
        Task<GetPagedListResponseModel<GetLocationResponseModel>> GetPagedListAsync(
            GetLocationsBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<LocationSortField> pagedListRequest);
        Task<IReadOnlyList<GetLocationLookupOptionResponseModel>> GetLookupOptionsAsync();
        Task<GetLocationResponseModel> GetAsync(int id);
        Task<IReadOnlyList<GetCityGeocodingCandidateResponseModel>> SearchCityGeocodingCandidatesAsync(
            GetCityGeocodingCandidatesRequestModel cityGeocodingCandidatesRequest);
        Task<IReadOnlyList<GetAddressCandidateResponseModel>> SearchAddressCandidatesAsync(
            GetAddressCandidatesRequestModel addressCandidatesRequest);
        Task<GetLocationResponseModel> EnsureCityLocationAsync(CreateLocationRequestModel model);
        Task<FillMissingLocationCoordinatesResponseModel> FillMissingCoordinatesAsync();
        Task<DeleteLocationResponseModel> DeleteAsync(int id);
        Task RestoreAsync(int id);
    }
}
