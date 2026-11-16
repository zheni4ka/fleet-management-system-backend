using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic.Specifications;
using FluentValidation;

namespace business_logic.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _auditLogRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IValidator<GetPagedListRequestModel<AuditLogSortField>> _pagedListRequestValidator;
        private readonly IValidator<GetAuditLogsBySearchFilterRequestModel> _searchFilterValidator;

        public AuditLogService(
            IRepository<AuditLog> auditLogRepository,
            ICurrentUserService currentUser,
            IValidator<GetPagedListRequestModel<AuditLogSortField>> pagedListRequestValidator,
            IValidator<GetAuditLogsBySearchFilterRequestModel> searchFilterValidator)
        {
            _auditLogRepository = auditLogRepository;
            _currentUser = currentUser;
            _pagedListRequestValidator = pagedListRequestValidator;
            _searchFilterValidator = searchFilterValidator;
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

        public async Task<GetPagedListResponseModel<GetAuditLogResponseModel>> GetPagedListAsync(
            GetAuditLogsBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<AuditLogSortField> pagedListRequest)
        {
            await _searchFilterValidator.ValidateAndThrowAsync(searchFilter);
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var auditLogsPagedListSpecification = new AuditLogSpecs.PagedList(searchFilter, pagedListRequest);
            var auditLogsPage = await _auditLogRepository.GetProjectedListBySpecAsync(auditLogsPagedListSpecification);
            var auditLogsTotalCount = await _auditLogRepository.CountBySpecAsync(auditLogsPagedListSpecification);

            return new GetPagedListResponseModel<GetAuditLogResponseModel>(
                Items: auditLogsPage,
                TotalCount: auditLogsTotalCount);
        }
    }
}
