using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class AuditLogChangesTests : IDisposable
    {
        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task UpdateAsync_AutoMarkChanged_StoresBeforeAndAfterValuesOfChangedPropertiesOnly()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА", mark: "Volvo");
            await _testHost.SeedAsync(auto);

            await _testHost.RunInNewScopeAsync<IAutoService>(autoService => autoService.UpdateAsync(new UpdateAutoRequestModel
            {
                Id = auto.Id,
                RowVersion = auto.RowVersion,
                DepotId = auto.DepotId,
                Mark = "Scania",
                Model = auto.Model,
                Color = auto.Color,
                Number = auto.Number,
                Capacity = auto.Capacity
            }));

            var autoUpdatedAuditLog = (await GetAuditLogsAsync()).Single(auditLog => auditLog.Action == AuditAction.AutoUpdated);
            Assert.Equal([new AuditLogPropertyChangeModel(PropertyName: nameof(Auto.Mark), PreviousValue: "Volvo", CurrentValue: "Scania")], autoUpdatedAuditLog.Changes);
        }

        [Fact]
        public async Task PurgeExpiredAuditLogsAsync_RowsOlderThanRetentionPeriod_DeletesOnlyExpiredRows()
        {
            var expiredAuditLog = new AuditLog
            {
                Action = AuditAction.AutoCreated,
                EntityId = "1",
                Time = DateTime.UtcNow.AddDays(-BusinessLogicTestHost.TestAuditLogRetentionPeriodInDays - 1)
            };
            var retainedAuditLog = new AuditLog
            {
                Action = AuditAction.AutoCreated,
                EntityId = "2",
                Time = DateTime.UtcNow.AddDays(-BusinessLogicTestHost.TestAuditLogRetentionPeriodInDays + 1)
            };
            await _testHost.SeedAsync(expiredAuditLog, retainedAuditLog);

            var purgedAuditLogsCount = await _testHost.RunInNewScopeAsync<IAuditLogService, int>(auditLogService => auditLogService.PurgeExpiredAuditLogsAsync());

            Assert.Equal(1, purgedAuditLogsCount);
            Assert.Equal(["2"], (await GetAuditLogsAsync()).Select(auditLog => auditLog.EntityId));
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }

        private async Task<IReadOnlyList<GetAuditLogResponseModel>> GetAuditLogsAsync()
        {
            var auditLogsPage = await _testHost.RunInNewScopeAsync<IAuditLogService, GetPagedListResponseModel<GetAuditLogResponseModel>>(
                auditLogService => auditLogService.GetPagedListAsync(
                    new GetAuditLogsBySearchFilterRequestModel(null, null, null),
                    new GetPagedListRequestModel<AuditLogSortField>(Page: 1, PageSize: 100, Sort: null, SortDirection: null, Search: null)));
            return auditLogsPage.Items;
        }
    }
}
