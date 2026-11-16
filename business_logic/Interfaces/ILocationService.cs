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
        Task<GetLocationResponseModel> CreateAsync(CreateLocationRequestModel model);
        Task DeleteAsync(int id);
    }
}
