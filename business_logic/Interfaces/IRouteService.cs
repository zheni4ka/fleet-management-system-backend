using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IRouteService
    {
        Task<IEnumerable<RouteDTO>> GetAllAsync();
        Task<RouteDTO> GetAsync(int id);
        Task<RouteDTO> CreateAsync(CreateRouteModel model);
        Task UpdateAsync(EditRouteModel model);
        Task DeleteAsync(int id);
    }
}
