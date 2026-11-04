using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationDTO>> GetAllAsync();
        Task<LocationDTO> GetAsync(int id);
        Task<LocationDTO> CreateAsync(CreateLocationModel model);
        Task DeleteAsync(int id);
    }
}
