using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IDriverService
    {
        Task<IEnumerable<DriverDTO>> GetAllAsync();
        Task<DriverDTO> GetAsync(int id);
        Task<DriverDTO> CreateAsync(CreateDriverModel model);
        Task EditAsync(EditDriverModel model);
        Task DeleteAsync(int id);
    }
}
