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
        private readonly IAutoStatusSyncService _autoStatusSyncService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateAutoMaintenanceRequestModel> _createValidator;
        private readonly IValidator<UpdateAutoMaintenanceRequestModel> _editValidator;

        public AutoMaintenanceService(
            IRepository<AutoMaintenance> maintenanceRepository,
            IRepository<Auto> autoRepository,
            IRepository<Route> routeRepository,
            IAuditLogService auditLogService,
            IAutoStatusSyncService autoStatusSyncService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateAutoMaintenanceRequestModel> createValidator,
            IValidator<UpdateAutoMaintenanceRequestModel> editValidator)
        {
            _maintenanceRepository = maintenanceRepository;
            _autoRepository = autoRepository;
            _routeRepository = routeRepository;
            _auditLogService = auditLogService;
            _autoStatusSyncService = autoStatusSyncService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _editValidator = editValidator;
        }

        public async Task<GetAutoMaintenanceResponseModel> GetAsync(int id)
        {
            var item = await GetMaintenanceOrThrowAsync(id);
            return _mapper.Map<GetAutoMaintenanceResponseModel>(item);
        }

        public async Task<IEnumerable<GetAutoMaintenanceResponseModel>> GetByAutoIdAsync(int autoId)
        {
            var items = await _maintenanceRepository.GetListBySpecAsync(new AutoMaintenanceSpecs.ByAutoId(autoId));
            return _mapper.Map<IEnumerable<GetAutoMaintenanceResponseModel>>(items);
        }

        public async Task<GetAutoMaintenanceResponseModel> CreateAsync(CreateAutoMaintenanceRequestModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);

            var auto = await _autoRepository.GetItemBySpecAsync(new AutoSpecs.ById(model.AutoId)) ?? throw NotFoundException.For<Auto>(model.AutoId);

            var affectedRoutes = await _routeRepository.GetListBySpecAsync(new RouteSpecs.ActiveByAutoFrom(model.AutoId, model.ServiceDate));
            foreach (var route in affectedRoutes)
            {
                route.Status = RouteStatus.Cancelled;
            }

            var maintenance = _mapper.Map<AutoMaintenance>(model);
            await _maintenanceRepository.InsertAsync(maintenance);

            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.MaintenanceCreated, maintenance.Id);
            await _autoStatusSyncService.SyncAutoStatusAsync(auto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GetAutoMaintenanceResponseModel>(maintenance);
        }

        public async Task UpdateAsync(UpdateAutoMaintenanceRequestModel model)
        {
            await _editValidator.ValidateAndThrowAsync(model);

            var maintenance = await GetMaintenanceOrThrowAsync(model.Id);
            var previousAutoId = maintenance.AutoId;

            _mapper.Map(model, maintenance);
            await _auditLogService.LogActionAsync(AuditAction.MaintenanceUpdated, maintenance.Id);
            await _unitOfWork.SaveChangesAsync();

            await _autoStatusSyncService.SyncAutoStatusAsync(maintenance.AutoId);
            if (previousAutoId != maintenance.AutoId)
            {
                await _autoStatusSyncService.SyncAutoStatusAsync(previousAutoId);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var maintenance = await GetMaintenanceOrThrowAsync(id);
            var maintenanceAutoId = maintenance.AutoId;

            _maintenanceRepository.Delete(maintenance);
            await _auditLogService.LogActionAsync(AuditAction.MaintenanceDeleted, id);
            await _unitOfWork.SaveChangesAsync();

            await _autoStatusSyncService.SyncAutoStatusAsync(maintenanceAutoId);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<AutoMaintenance> GetMaintenanceOrThrowAsync(int id)
        {
            return await _maintenanceRepository.GetItemBySpecAsync(new AutoMaintenanceSpecs.ById(id)) ?? throw NotFoundException.For<AutoMaintenance>(id);
        }
    }
}
