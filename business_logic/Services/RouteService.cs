using AutoMapper;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic.Specifications;
using FluentValidation;

namespace business_logic.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRepository<Route> _routeRepository;
        private readonly IRepository<Auto> _autoRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateRouteModel> _createValidator;
        private readonly IValidator<EditRouteModel> _editValidator;

        public RouteService(
            IRepository<Route> routeRepository,
            IRepository<Auto> autoRepository,
            IRepository<Driver> driverRepository,
            IRepository<Location> locationRepository,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateRouteModel> createValidator,
            IValidator<EditRouteModel> editValidator)
        {
            _routeRepository = routeRepository;
            _autoRepository = autoRepository;
            _driverRepository = driverRepository;
            _locationRepository = locationRepository;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _editValidator = editValidator;
        }

        public async Task<IEnumerable<RouteDTO>> GetAllAsync()
        {
            var routes = await _routeRepository.GetListBySpecAsync(new RouteSpecs.All());
            return _mapper.Map<IEnumerable<RouteDTO>>(routes);
        }

        public async Task<RouteDTO> GetAsync(int id)
        {
            var route = await GetRouteOrThrowAsync(id);
            return _mapper.Map<RouteDTO>(route);
        }

        public async Task<RouteDTO> CreateAsync(CreateRouteModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);

            var auto = await GetUsableAutoAsync(model.AutoId);
            await EnsureDriverExistsAsync(model.DriverId);
            await EnsureLocationsExistAsync(model.StartLocationId, model.DestinationLocationId);
            await EnsureResourcesAreFreeAsync(model.AutoId, model.DriverId, model.DepartureTime, model.ArrivalTime);

            var route = _mapper.Map<Route>(model);
            await _routeRepository.InsertAsync(route);

            // Маршрут, створений одразу в статусі InProgress, займає автомобіль.
            if (route.Status == RouteStatus.InProgress)
            {
                auto.Status = AutoStatus.InService;
            }

            // Перше збереження потрібне, щоб БД згенерувала route.Id для запису в аудит.
            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.RouteCreated, route.Id);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<RouteDTO>(route);
        }

        public async Task UpdateAsync(EditRouteModel model)
        {
            await _editValidator.ValidateAndThrowAsync(model);

            var route = await GetRouteOrThrowAsync(model.Id);

            // Старі значення запам'ятовуються до мапінгу: за ними визначається, чи треба звільнити попереднє авто.
            int oldAutoId = route.AutoId;
            var oldStatus = route.Status;

            // Заборона на авто в ремонті діє лише при заміні авто; поточне авто маршруту можна лишити,
            // щоб диспетчер міг, наприклад, скасувати рейс авто, яке вже потрапило на ремонт.
            var newAuto = oldAutoId == model.AutoId
                ? await _autoRepository.GetByIdAsync(model.AutoId) ?? throw NotFoundException.For<Auto>(model.AutoId)
                : await GetUsableAutoAsync(model.AutoId);
            await EnsureDriverExistsAsync(model.DriverId);
            await EnsureLocationsExistAsync(model.StartLocationId!.Value, model.DestinationLocationId!.Value);

            // Час рейсу при редагуванні не змінюється, тому зайнятість перевіряється за поточним інтервалом,
            // а сам маршрут виключається, щоб не конфліктувати з собою.
            await EnsureResourcesAreFreeAsync(model.AutoId, model.DriverId, route.DepartureTime, route.ArrivalTime, route.Id);

            _mapper.Map(model, route);

            // Попереднє авто звільняється, якщо рейс був активним і тепер воно його більше не обслуговує:
            // авто замінили або маршрут перейшов зі статусу InProgress.
            if (oldStatus == RouteStatus.InProgress && (oldAutoId != route.AutoId || route.Status != RouteStatus.InProgress))
            {
                var oldAuto = await _autoRepository.GetByIdAsync(oldAutoId);
                if (oldAuto is { Status: AutoStatus.InService })
                {
                    oldAuto.Status = AutoStatus.Available;
                }
            }

            if (route.Status == RouteStatus.InProgress)
            {
                newAuto.Status = AutoStatus.InService;
            }

            await _auditLogService.LogActionAsync(AuditAction.RouteUpdated, route.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var route = await GetRouteOrThrowAsync(id);

            // Видалення активного рейсу звільняє авто; статус InService перевіряється, щоб не скинути UnderMaintenance.
            if (route.Status == RouteStatus.InProgress)
            {
                var auto = await _autoRepository.GetByIdAsync(route.AutoId);
                if (auto is { Status: AutoStatus.InService })
                {
                    auto.Status = AutoStatus.Available;
                }
            }

            _routeRepository.Delete(route);
            await _auditLogService.LogActionAsync(AuditAction.RouteDeleted, id);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Route> GetRouteOrThrowAsync(int id)
        {
            return await _routeRepository.GetByIdAsync(id) ?? throw NotFoundException.For<Route>(id);
        }

        private async Task<Auto> GetUsableAutoAsync(int autoId)
        {
            var auto = await _autoRepository.GetByIdAsync(autoId) ?? throw NotFoundException.For<Auto>(autoId);
            if (auto.Status == AutoStatus.UnderMaintenance)
            {
                throw new BusinessRuleException("Автомобіль на ремонті.");
            }
            return auto;
        }

        private async Task EnsureDriverExistsAsync(int driverId)
        {
            if (await _driverRepository.GetByIdAsync(driverId) == null)
            {
                throw NotFoundException.For<Driver>(driverId);
            }
        }

        private async Task EnsureLocationsExistAsync(params int[] locationIds)
        {
            foreach (var locationId in locationIds)
            {
                if (await _locationRepository.GetByIdAsync(locationId) == null)
                {
                    throw NotFoundException.For<Location>(locationId);
                }
            }
        }

        // Перевірка перетину інтервалів виконується в БД через специфікації. Скасовані й завершені рейси
        // ресурс не займають; excludeRouteId виключає маршрут, що редагується.
        private async Task EnsureResourcesAreFreeAsync(int autoId, int driverId, DateTime departure, DateTime arrival, int? excludeRouteId = null)
        {
            if (await _routeRepository.AnyBySpecAsync(new RouteSpecs.OverlappingForAuto(autoId, departure, arrival, excludeRouteId)))
            {
                throw new BusinessRuleException("Автомобіль вже зайнятий на іншому рейсі у цей час.");
            }

            if (await _routeRepository.AnyBySpecAsync(new RouteSpecs.OverlappingForDriver(driverId, departure, arrival, excludeRouteId)))
            {
                throw new BusinessRuleException("Водій вже зайнятий на іншому рейсі у цей час.");
            }
        }
    }
}
