using AutoMapper;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic.Specifications;
using FluentValidation;

namespace business_logic.Services
{
    public class AutoService : IAutoService
    {
        private readonly IRepository<Auto> _autoRepository;
        private readonly IRepository<Route> _routeRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateAutoModel> _createValidator;
        private readonly IValidator<EditAutoModel> _editValidator;

        public AutoService(
            IRepository<Auto> autoRepository,
            IRepository<Route> routeRepository,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateAutoModel> createValidator,
            IValidator<EditAutoModel> editValidator)
        {
            _autoRepository = autoRepository;
            _routeRepository = routeRepository;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _editValidator = editValidator;
        }

        public async Task<IEnumerable<AutoDTO>> GetAllAsync()
        {
            var autos = await _autoRepository.GetListBySpecAsync(new AutoSpecs.All());
            return _mapper.Map<IEnumerable<AutoDTO>>(autos);
        }

        public async Task<AutoDTO> GetAsync(int id)
        {
            var auto = await GetAutoOrThrowAsync(id);
            return _mapper.Map<AutoDTO>(auto);
        }

        public async Task<AutoDTO> CreateAsync(CreateAutoModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);
            await EnsureNumberIsUniqueAsync(model.Number);

            var auto = _mapper.Map<Auto>(model);
            await _autoRepository.InsertAsync(auto);
            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.AutoCreated, auto.Id);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AutoDTO>(auto);
        }

        public async Task EditAsync(EditAutoModel model)
        {
            await _editValidator.ValidateAndThrowAsync(model);

            var auto = await GetAutoOrThrowAsync(model.Id);
            await EnsureNumberIsUniqueAsync(model.Number, model.Id);

            _mapper.Map(model, auto);
            await _auditLogService.LogActionAsync(AuditAction.AutoUpdated, auto.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var auto = await GetAutoOrThrowAsync(id);

            if (await _routeRepository.AnyBySpecAsync(new RouteSpecs.ByAutoId(id)))
            {
                throw new BusinessRuleException("Unable to delete an auto that is used in routes.");
            }

            _autoRepository.Delete(auto);
            await _auditLogService.LogActionAsync(AuditAction.AutoDeleted, id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int autoId, AutoStatus newStatus)
        {
            if (!Enum.IsDefined(newStatus))
            {
                throw new BusinessRuleException("Unknown auto status.");
            }

            var auto = await GetAutoOrThrowAsync(autoId);
            auto.Status = newStatus;
            await _auditLogService.LogActionAsync(AuditAction.AutoStatusChanged, auto.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Auto> GetAutoOrThrowAsync(int id)
        {
            return await _autoRepository.GetByIdAsync(id) ?? throw NotFoundException.For<Auto>(id);
        }

        private async Task EnsureNumberIsUniqueAsync(string number, int? excludeId = null)
        {
            var existing = await _autoRepository.GetItemBySpecAsync(new AutoSpecs.ByNumber(number));
            if (existing != null && existing.Id != excludeId)
            {
                throw new BusinessRuleException($"Auto with number '{number}' already exists.");
            }
        }
    }
}
