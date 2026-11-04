using AutoMapper;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic.Specifications;
using FluentValidation;

namespace business_logic.Services
{
    public class AutoMaintenanceService : IAutoMaintenanceService
    {
        private readonly IRepository<AutoMaintenance> _maintenanceRepository;
        private readonly IRepository<Auto> _autoRepository;
        private readonly IRepository<Route> _routeRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateAutoMaintenanceModel> _createValidator;
        private readonly IValidator<EditAutoMaintenanceModel> _editValidator;

        public AutoMaintenanceService(
            IRepository<AutoMaintenance> maintenanceRepository,
            IRepository<Auto> autoRepository,
            IRepository<Route> routeRepository,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateAutoMaintenanceModel> createValidator,
            IValidator<EditAutoMaintenanceModel> editValidator)
        {
            _maintenanceRepository = maintenanceRepository;
            _autoRepository = autoRepository;
            _routeRepository = routeRepository;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _editValidator = editValidator;
        }

        public async Task<IEnumerable<AutoMaintenanceDTO>> GetAllAsync()
        {
            var items = await _maintenanceRepository.GetListBySpecAsync(new AutoMaintenanceSpecs.All());
            return _mapper.Map<IEnumerable<AutoMaintenanceDTO>>(items);
        }

        public async Task<AutoMaintenanceDTO> GetAsync(int id)
        {
            var item = await GetMaintenanceOrThrowAsync(id);
            return _mapper.Map<AutoMaintenanceDTO>(item);
        }

        public async Task<IEnumerable<AutoMaintenanceDTO>> GetByAutoIdAsync(int autoId)
        {
            var items = await _maintenanceRepository.GetListBySpecAsync(new AutoMaintenanceSpecs.ByAutoId(autoId));
            return _mapper.Map<IEnumerable<AutoMaintenanceDTO>>(items);
        }

        public async Task<AutoMaintenanceDTO> CreateAsync(CreateAutoMaintenanceModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);

            var auto = await _autoRepository.GetByIdAsync(model.AutoId) ?? throw new NotFoundException("Автомобіль не знайдено");

            // Заплановані та активні рейси авто, що відправляються з дати обслуговування, скасовуються
            // в тій самій транзакції, що й створення запису про ремонт.
            var affectedRoutes = await _routeRepository.GetListBySpecAsync(new RouteSpecs.ActiveByAutoFrom(model.AutoId, model.ServiceDate));
            foreach (var route in affectedRoutes)
            {
                route.Status = RouteStatus.Cancelled;
            }

            var maintenance = _mapper.Map<AutoMaintenance>(model);
            await _maintenanceRepository.InsertAsync(maintenance);
            auto.Status = AutoStatus.UnderMaintenance;

            // Перше збереження потрібне, щоб БД згенерувала maintenance.Id для запису в аудит.
            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.MaintenanceCreated, maintenance.Id);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AutoMaintenanceDTO>(maintenance);
        }

        public async Task UpdateAsync(EditAutoMaintenanceModel model)
        {
            await _editValidator.ValidateAndThrowAsync(model);

            var maintenance = await GetMaintenanceOrThrowAsync(model.Id);
            _mapper.Map(model, maintenance);

            if (maintenance.IsCompleted)
            {
                await ReleaseAutoAsync(maintenance.AutoId);
            }

            await _auditLogService.LogActionAsync(AuditAction.MaintenanceUpdated, maintenance.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var maintenance = await GetMaintenanceOrThrowAsync(id);

            // Авто ще тримає незавершене обслуговування, тож при його видаленні авто повертається в роботу.
            if (!maintenance.IsCompleted)
            {
                await ReleaseAutoAsync(maintenance.AutoId);
            }

            _maintenanceRepository.Delete(maintenance);
            await _auditLogService.LogActionAsync(AuditAction.MaintenanceDeleted, id);
            await _unitOfWork.SaveChangesAsync();
        }

        // Скидає лише статус UnderMaintenance, щоб не звільнити авто, яке тим часом поставили на рейс (InService).
        private async Task ReleaseAutoAsync(int autoId)
        {
            var auto = await _autoRepository.GetByIdAsync(autoId);
            if (auto is { Status: AutoStatus.UnderMaintenance })
            {
                auto.Status = AutoStatus.Available;
            }
        }

        private async Task<AutoMaintenance> GetMaintenanceOrThrowAsync(int id)
        {
            return await _maintenanceRepository.GetByIdAsync(id) ?? throw NotFoundException.For<AutoMaintenance>(id);
        }
    }
}
