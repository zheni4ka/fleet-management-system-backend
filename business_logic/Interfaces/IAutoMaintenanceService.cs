using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IAutoMaintenanceService
    {
        Task<GetAutoMaintenanceResponseModel> GetAsync(int id);
        Task<IEnumerable<GetAutoMaintenanceResponseModel>> GetByAutoIdAsync(int autoId);
        Task<GetAutoMaintenanceResponseModel> CreateAsync(CreateAutoMaintenanceRequestModel model);
        Task UpdateAsync(UpdateAutoMaintenanceRequestModel model);
        Task DeleteAsync(int id);
    }
}
