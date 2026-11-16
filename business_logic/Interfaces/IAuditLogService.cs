using business_logic.DTOs;
using business_logic.Entities;

namespace business_logic.Interfaces
{
    public interface IAuditLogService
    {
        Task LogActionAsync(AuditAction action, object? entityId = null);
        Task<int> PurgeExpiredAuditLogsAsync();

        Task<GetPagedListResponseModel<GetAuditLogResponseModel>> GetPagedListAsync(
            GetAuditLogsBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<AuditLogSortField> pagedListRequest);
    }
}
