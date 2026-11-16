using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Helpers;
using business_logic.Interfaces;
using business_logic.Settings;
using business_logic.Specifications;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace business_logic.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _auditLogRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<GetPagedListRequestModel<AuditLogSortField>> _pagedListRequestValidator;
        private readonly IValidator<GetAuditLogsBySearchFilterRequestModel> _searchFilterValidator;
        private readonly IOptions<AuditLogRetentionSettings> _auditLogRetentionSettingsOptions;

        public AuditLogService(
            IRepository<AuditLog> auditLogRepository,
            ICurrentUserService currentUser,
            IValidator<GetPagedListRequestModel<AuditLogSortField>> pagedListRequestValidator,
            IValidator<GetAuditLogsBySearchFilterRequestModel> searchFilterValidator,
            IOptions<AuditLogRetentionSettings> auditLogRetentionSettingsOptions)
        {
            _auditLogRepository = auditLogRepository;
            _currentUser = currentUser;
            _pagedListRequestValidator = pagedListRequestValidator;
            _searchFilterValidator = searchFilterValidator;
            _auditLogRetentionSettingsOptions = auditLogRetentionSettingsOptions;
        }

        public async Task LogActionAsync(AuditAction action, object? entityId = null)
        {
            await _auditLogRepository.InsertAsync(new AuditLog
            {
                DispatcherId = _currentUser.UserId,
                Action = action,
                EntityId = entityId?.ToString(),
                Time = DateTime.UtcNow
            });
        }

        public async Task<int> PurgeExpiredAuditLogsAsync()
        {
            var retentionCutoffUtcTime = DateTime.UtcNow.AddDays(-_auditLogRetentionSettingsOptions.Value.RetentionPeriodInDays);
            return await _auditLogRepository.DeleteBySpecAsync(new AuditLogSpecs.OlderThan(retentionCutoffUtcTime));
        }

        public async Task<GetPagedListResponseModel<GetAuditLogResponseModel>> GetPagedListAsync(
            GetAuditLogsBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<AuditLogSortField> pagedListRequest)
        {
            await _searchFilterValidator.ValidateAndThrowAsync(searchFilter);
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var auditLogsPagedListSpecification = new AuditLogSpecs.PagedList(searchFilter, pagedListRequest);
            var auditLogPageRows = await _auditLogRepository.GetProjectedListBySpecAsync(auditLogsPagedListSpecification);
            var auditLogsTotalCount = await _auditLogRepository.CountBySpecAsync(auditLogsPagedListSpecification);

            return new GetPagedListResponseModel<GetAuditLogResponseModel>(
                Items: auditLogPageRows.Select(auditLogPageRow => new GetAuditLogResponseModel
                {
                    Id = auditLogPageRow.Id,
                    DispatcherId = auditLogPageRow.DispatcherId,
                    DispatcherUsername = auditLogPageRow.DispatcherUsername,
                    Time = auditLogPageRow.Time,
                    Action = auditLogPageRow.Action,
                    EntityId = auditLogPageRow.EntityId,
                    Changes = AuditLogChangesSerializer.DeserializeAuditLogChanges(auditLogPageRow.ChangesJson)
                }).ToList(),
                TotalCount: auditLogsTotalCount);
        }
    }
}
