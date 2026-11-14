using business_logic.DTOs;
using business_logic.Entities;

namespace business_logic.Interfaces
{
    public interface IAutoService
    {
        Task<IEnumerable<AutoDTO>> GetAllAsync();
        Task<GetPagedListResponseModel<AutoDTO>> GetPagedListAsync(
            GetAutosBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<AutoSortField> pagedListRequest);
        Task<AutoDTO> GetAsync(int id);
        Task<AutoDTO> CreateAsync(CreateAutoModel model);
        Task EditAsync(EditAutoModel model);
        Task DeleteAsync(int id);
        Task UpdateStatusAsync(int autoId, AutoStatus newStatus);
    }
}
