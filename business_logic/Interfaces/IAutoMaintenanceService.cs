using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IAutoMaintenanceService
    {
        Task<IEnumerable<AutoMaintenanceDTO>> GetAllAsync();
        Task<AutoMaintenanceDTO> GetAsync(int id);
        Task<IEnumerable<AutoMaintenanceDTO>> GetByAutoIdAsync(int autoId);
        Task<AutoMaintenanceDTO> CreateAsync(CreateAutoMaintenanceModel model);
        Task UpdateAsync(EditAutoMaintenanceModel model);
        Task DeleteAsync(int id);
    }
}
