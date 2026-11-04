using AutoMapper;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic.Specifications;

namespace business_logic.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _auditLogRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public AuditLogService(IRepository<AuditLog> auditLogRepository, ICurrentUserService currentUser, IMapper mapper)
        {
            _auditLogRepository = auditLogRepository;
            _currentUser = currentUser;
            _mapper = mapper;
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
    }
}
