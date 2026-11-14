using business_logic.Entities;

namespace business_logic.Interfaces
{
    public interface IAutoStatusSyncService
    {
        Task SyncAutoStatusAsync(Auto auto);
        Task SyncAutoStatusAsync(int autoId);
        Task<int> SyncAutosWithDueMaintenanceAsync();
    }
}
