using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class AuditLogTimeTests : IDisposable
    {
        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task GetPagedListAsync_LoggedAction_ReturnsTimeWithUtcKind()
        {
            var timeBeforeAutoCreation = DateTime.UtcNow;
            await _testHost.RunInNewScopeAsync<IAutoService, GetAutoResponseModel>(autoService => autoService.CreateAsync(new CreateAutoRequestModel
            {
                DepotId = DepotConstraints.DefaultDepotId,
                Mark = "Volvo",
                Model = "FH16",
                Color = "Білий",
                Number = "АА1111АА",
                Capacity = 20
            }));

            var auditLogsPage = await _testHost.RunInNewScopeAsync<IAuditLogService, GetPagedListResponseModel<GetAuditLogResponseModel>>(
                auditLogService => auditLogService.GetPagedListAsync(
                    new GetAuditLogsBySearchFilterRequestModel(Action: null, TimeFromDate: null, TimeToDate: null),
                    new GetPagedListRequestModel<AuditLogSortField>(Page: 1, PageSize: 10, Sort: null, SortDirection: null, Search: null)));

            var autoCreatedAuditLog = Assert.Single(auditLogsPage.Items);
            Assert.Equal(DateTimeKind.Utc, autoCreatedAuditLog.Time.Kind);
            Assert.InRange(autoCreatedAuditLog.Time, timeBeforeAutoCreation, DateTime.UtcNow);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
