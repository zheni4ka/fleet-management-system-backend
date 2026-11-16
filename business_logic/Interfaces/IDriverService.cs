using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IDriverService
    {
        Task<GetPagedListResponseModel<GetDriverResponseModel>> GetPagedListAsync(GetPagedListRequestModel<DriverSortField> pagedListRequest);
        Task<IReadOnlyList<GetDriverLookupOptionResponseModel>> GetLookupOptionsAsync();
        Task<GetDriverResponseModel> GetAsync(int id);
        Task<GetDriverResponseModel> CreateAsync(CreateDriverRequestModel model);
        Task UpdateAsync(UpdateDriverRequestModel model);
        Task DeleteAsync(int id);
    }
}
