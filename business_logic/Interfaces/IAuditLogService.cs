using business_logic.DTOs;
using business_logic.Entities;

namespace business_logic.Interfaces
{
    public interface IAuditLogService
    {
        Task LogActionAsync(AuditAction action, object? entityId = null);

        Task<IEnumerable<AuditLogDTO>> GetLogsAsync();

        Task<GetPagedListResponseModel<AuditLogDTO>> GetPagedListAsync(GetPagedListRequestModel<AuditLogSortField> pagedListRequest, AuditAction? action);
    }
}
