using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IAutoService
    {
        Task<GetPagedListResponseModel<GetAutoResponseModel>> GetPagedListAsync(
            GetAutosBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<AutoSortField> pagedListRequest);
        Task<IReadOnlyList<GetAutoLookupOptionResponseModel>> GetLookupOptionsAsync();
        Task<GetAutoResponseModel> GetAsync(int id);
        Task<GetAutoResponseModel> CreateAsync(CreateAutoRequestModel model);
        Task UpdateAsync(UpdateAutoRequestModel model);
        Task<DeleteAutoResponseModel> DeleteAsync(int id);
        Task RestoreAsync(int id);
    }
}
