using AutoMapper;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
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

        public AuditLogService(
            IRepository<AuditLog> auditLogRepository,
            ICurrentUserService currentUser,
            IMapper mapper,
            IValidator<GetPagedListRequestModel<AuditLogSortField>> pagedListRequestValidator)
        {
            _auditLogRepository = auditLogRepository;
            _currentUser = currentUser;
            _mapper = mapper;
            _pagedListRequestValidator = pagedListRequestValidator;
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
            GetPagedListRequestModel<AuditLogSortField> pagedListRequest,
            AuditAction? action)
        {
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);
            if (action.HasValue && !Enum.IsDefined(action.Value))
            {
                throw new BusinessRuleException($"Дія журналу '{action.Value}' не підтримується.");
            }

            var auditLogsPagedListSpecification = new AuditLogSpecs.PagedList(pagedListRequest, action);
            var auditLogsPage = await _auditLogRepository.GetProjectedListBySpecAsync(auditLogsPagedListSpecification);
            var auditLogsTotalCount = await _auditLogRepository.CountBySpecAsync(auditLogsPagedListSpecification);

            return new GetPagedListResponseModel<AuditLogDTO>(
                Items: auditLogsPage,
                TotalCount: auditLogsTotalCount);
        }
    }
}
