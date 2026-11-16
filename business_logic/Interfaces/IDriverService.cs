using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IDriverService
    {
        Task<GetPagedListResponseModel<GetDriverResponseModel>> GetPagedListAsync(
            GetDriversBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<DriverSortField> pagedListRequest);
        Task<IReadOnlyList<GetDriverLookupOptionResponseModel>> GetLookupOptionsAsync();
        Task<GetDriverResponseModel> GetAsync(int id);
        Task<GetDriverResponseModel> CreateAsync(CreateDriverRequestModel model);
        Task UpdateAsync(UpdateDriverRequestModel model);
        Task<DeleteDriverResponseModel> DeleteAsync(int id);
        Task RestoreAsync(int id);
    }
}
