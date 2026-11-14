using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationDTO>> GetAllAsync();
        Task<GetPagedListResponseModel<LocationDTO>> GetPagedListAsync(GetPagedListRequestModel<LocationSortField> pagedListRequest);
        Task<LocationDTO> GetAsync(int id);
        Task<LocationDTO> CreateAsync(CreateLocationModel model);
        Task DeleteAsync(int id);
    }
}
