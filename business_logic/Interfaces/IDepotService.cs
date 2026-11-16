using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IDepotService
    {
        Task<IReadOnlyList<GetDepotResponseModel>> GetDepotsAsync();
        Task<GetDepotResponseModel> CreateAsync(CreateDepotRequestModel model);
        Task UpdateAsync(UpdateDepotRequestModel model);
        Task DeleteAsync(int id);
        Task EnsureDepotsExistAsync(IReadOnlyCollection<int> depotIds);
    }
}
