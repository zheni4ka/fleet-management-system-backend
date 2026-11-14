using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IValidator<GetPagedListRequestModel<AuditLogSortField>> _pagedListRequestValidator;
        private readonly IValidator<GetAuditLogsBySearchFilterRequestModel> _searchFilterValidator;

        public AuditLogService(
            IRepository<AuditLog> auditLogRepository,
            ICurrentUserService currentUser,
            IMapper mapper,
            IValidator<GetPagedListRequestModel<AuditLogSortField>> pagedListRequestValidator,
            IValidator<GetAuditLogsBySearchFilterRequestModel> searchFilterValidator)
        {
            _auditLogRepository = auditLogRepository;
            _currentUser = currentUser;
            _mapper = mapper;
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

        public async Task<IEnumerable<AuditLogDTO>> GetLogsAsync()
        {
            var logs = await _auditLogRepository.GetListBySpecAsync(new AuditLogSpecs.All());
            return _mapper.Map<IEnumerable<AuditLogDTO>>(logs);
        }

        public async Task<GetPagedListResponseModel<AuditLogDTO>> GetPagedListAsync(
            GetAuditLogsBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<AuditLogSortField> pagedListRequest)
        {
            await _searchFilterValidator.ValidateAndThrowAsync(searchFilter);
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var auditLogsPagedListSpecification = new AuditLogSpecs.PagedList(searchFilter, pagedListRequest);
            var auditLogsPage = await _auditLogRepository.GetProjectedListBySpecAsync(auditLogsPagedListSpecification);
            var auditLogsTotalCount = await _auditLogRepository.CountBySpecAsync(auditLogsPagedListSpecification);

            return new GetPagedListResponseModel<AuditLogDTO>(
                Items: auditLogsPage,
                TotalCount: auditLogsTotalCount);
        }
    }
}
