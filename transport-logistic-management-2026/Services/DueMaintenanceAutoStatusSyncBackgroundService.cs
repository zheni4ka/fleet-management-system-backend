using business_logic.Constants;
using business_logic.Interfaces;

namespace transport_logistic_management_2026.Services
{
    public class DueMaintenanceAutoStatusSyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DueMaintenanceAutoStatusSyncBackgroundService> _logger;

        public DueMaintenanceAutoStatusSyncBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DueMaintenanceAutoStatusSyncBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var syncIntervalTimer = new PeriodicTimer(AutoStatusSyncConstraints.DueMaintenanceSyncInterval);
            do
            {
                await SyncAutosWithDueMaintenanceAsync();
            }
            while (await syncIntervalTimer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task SyncAutosWithDueMaintenanceAsync()
        {
            try
            {
                using var serviceScope = _serviceScopeFactory.CreateScope();
                var databaseApplicationLockService = serviceScope.ServiceProvider.GetRequiredService<IDatabaseApplicationLockService>();
                await using var dueMaintenanceSyncJobLock = await databaseApplicationLockService.TryAcquireSessionExclusiveLockWithoutWaitingAsync(
                    ApplicationLockResourceNames.DueMaintenanceAutoStatusSyncJob);
                if (dueMaintenanceSyncJobLock is null)
                {
                    _logger.LogInformation("Skipped due maintenance auto status sync because another instance is running it.");
                    return;
                }

                var autoStatusSyncService = serviceScope.ServiceProvider.GetRequiredService<IAutoStatusSyncService>();
                var autosMovedUnderMaintenanceCount = await autoStatusSyncService.SyncAutosWithDueMaintenanceAsync();
                if (autosMovedUnderMaintenanceCount > 0)
                {
                    _logger.LogInformation("Moved {AutosCount} autos to UnderMaintenance because of due maintenance.", autosMovedUnderMaintenanceCount);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Due maintenance auto status sync failed.");
            }
        }
    }
}
