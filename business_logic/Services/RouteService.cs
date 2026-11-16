using AutoMapper;
using business_logic.Constants;
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
        private readonly IRepository<AutoMaintenance> _maintenanceRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IAutoStatusSyncService _autoStatusSyncService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateRouteModel> _createValidator;
        private readonly IValidator<EditRouteModel> _editValidator;
        private readonly IValidator<GetRoutesBySearchFilterRequestModel> _searchFilterValidator;
        private readonly IValidator<GetRoutesPeriodReportRequestModel> _periodReportValidator;
        private readonly IValidator<GetPagedListRequestModel<RouteSortField>> _pagedListRequestValidator;

        public RouteService(
            IRepository<Route> routeRepository,
            IRepository<Auto> autoRepository,
            IRepository<Driver> driverRepository,
            IRepository<Location> locationRepository,
            IRepository<AutoMaintenance> maintenanceRepository,
            IAuditLogService auditLogService,
            IAutoStatusSyncService autoStatusSyncService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateRouteModel> createValidator,
            IValidator<EditRouteModel> editValidator,
            IValidator<GetRoutesBySearchFilterRequestModel> searchFilterValidator,
            IValidator<GetRoutesPeriodReportRequestModel> periodReportValidator,
            IValidator<GetPagedListRequestModel<RouteSortField>> pagedListRequestValidator)
        {
            _routeRepository = routeRepository;
            _autoRepository = autoRepository;
            _driverRepository = driverRepository;
            _locationRepository = locationRepository;
            _maintenanceRepository = maintenanceRepository;
            _auditLogService = auditLogService;
            _autoStatusSyncService = autoStatusSyncService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _editValidator = editValidator;
            _searchFilterValidator = searchFilterValidator;
            _periodReportValidator = periodReportValidator;
            _pagedListRequestValidator = pagedListRequestValidator;
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

        public async Task<IEnumerable<RouteDTO>> GetRoutesBySearchFilterAsync(GetRoutesBySearchFilterRequestModel searchFilter)
        {
            await _searchFilterValidator.ValidateAndThrowAsync(searchFilter);

            var filteredRoutes = await _routeRepository.GetListBySpecAsync(new RouteSpecs.BySearchFilter(searchFilter));
            return _mapper.Map<IEnumerable<RouteDTO>>(filteredRoutes);
        }

        public async Task<GetPagedListResponseModel<RouteDTO>> GetRoutesPagedListBySearchFilterAsync(
            GetRoutesBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<RouteSortField> pagedListRequest)
        {
            await _searchFilterValidator.ValidateAndThrowAsync(searchFilter);
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var routesPagedListSpecification = new RouteSpecs.PagedListBySearchFilter(searchFilter, pagedListRequest);
            var routesPage = await _routeRepository.GetProjectedListBySpecAsync(routesPagedListSpecification);
            var routesTotalCount = await _routeRepository.CountBySpecAsync(routesPagedListSpecification);

            return new GetPagedListResponseModel<RouteDTO>(
                Items: routesPage,
                TotalCount: routesTotalCount);
        }

        public async Task<GetRoutesPeriodReportResponseModel> GetRoutesPeriodReportAsync(GetRoutesPeriodReportRequestModel periodReportRequest)
        {
            await _periodReportValidator.ValidateAndThrowAsync(periodReportRequest);

            var periodStartDateTime = periodReportRequest.PeriodStartDate.ToDateTime(TimeOnly.MinValue);
            var periodEndExclusiveDateTime = periodReportRequest.PeriodEndDate.AddDays(1).ToDateTime(TimeOnly.MinValue);
            var periodRoutesSpecification = new RouteSpecs.ByDeparturePeriod(periodStartDateTime, periodEndExclusiveDateTime);

            var periodRoutes = await _routeRepository.GetProjectedListBySpecAsync(periodRoutesSpecification);
            var periodRoutesCountsByStatus = await _routeRepository.GetCountsByGroupKeyBySpecAsync(periodRoutesSpecification, route => route.Status);
            var involvedAutosCount = await _autoRepository.CountBySpecAsync(
                new AutoSpecs.WithRoutesDepartingInPeriod(periodStartDateTime, periodEndExclusiveDateTime));
            var driverRoutesCountSummaries = await _driverRepository.GetProjectedListBySpecAsync(
                new DriverSpecs.RoutesPeriodReportSummaries(periodStartDateTime, periodEndExclusiveDateTime));

            var completedPeriodRoutes = periodRoutes.Where(route => route.Status == RouteStatus.Completed).ToList();
            var completedRoutesDrivingHoursByDriverId = completedPeriodRoutes
                .GroupBy(route => route.DriverId)
                .ToDictionary(driverCompletedRoutes => driverCompletedRoutes.Key, GetDrivingHours);
            var driverSummaries = driverRoutesCountSummaries
                .Select(driverSummary => driverSummary with
                {
                    CompletedRoutesDrivingHours = completedRoutesDrivingHoursByDriverId.GetValueOrDefault(driverSummary.DriverId)
                })
                .ToList();

            return new GetRoutesPeriodReportResponseModel(
                PeriodStartDate: periodReportRequest.PeriodStartDate,
                PeriodEndDate: periodReportRequest.PeriodEndDate,
                TotalRoutesCount: periodRoutes.Count,
                PlannedRoutesCount: periodRoutesCountsByStatus.GetValueOrDefault(RouteStatus.Planned),
                InProgressRoutesCount: periodRoutesCountsByStatus.GetValueOrDefault(RouteStatus.InProgress),
                CompletedRoutesCount: periodRoutesCountsByStatus.GetValueOrDefault(RouteStatus.Completed),
                CancelledRoutesCount: periodRoutesCountsByStatus.GetValueOrDefault(RouteStatus.Cancelled),
                CompletedRoutesDrivingHours: GetDrivingHours(completedPeriodRoutes),
                InvolvedAutosCount: involvedAutosCount,
                InvolvedDriversCount: driverSummaries.Count,
                DriverSummaries: driverSummaries,
                Routes: periodRoutes);
        }

        public async Task<RouteDTO> CreateAsync(CreateRouteModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);

            var auto = await GetUsableAutoAsync(model.AutoId);
            await EnsureDriverExistsAsync(model.DriverId);
            await EnsureLocationsExistAsync(model.StartLocationId, model.DestinationLocationId);

            if (RouteStatusGroups.Active.Contains(model.Status))
            {
                await EnsureAssignmentHasNoConflictsAsync(model.AutoId, model.DriverId, model.DepartureTime, model.ArrivalTime);
            }

            var route = _mapper.Map<Route>(model);
            await _routeRepository.InsertAsync(route);

            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.RouteCreated, route.Id);
            await _autoStatusSyncService.SyncAutoStatusAsync(auto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<RouteDTO>(route);
        }

        public async Task UpdateAsync(EditRouteModel model)
        {
            await _editValidator.ValidateAndThrowAsync(model);

            var route = await GetRouteOrThrowAsync(model.Id);
            var previousAutoId = route.AutoId;

            var assignedAuto = previousAutoId == model.AutoId
                ? await _autoRepository.GetItemBySpecAsync(new AutoSpecs.ById(model.AutoId)) ?? throw NotFoundException.For<Auto>(model.AutoId)
                : await GetUsableAutoAsync(model.AutoId);
            await EnsureDriverExistsAsync(model.DriverId);
            await EnsureLocationsExistAsync(model.StartLocationId, model.DestinationLocationId);

            if (RouteStatusGroups.Active.Contains(model.Status))
            {
                await EnsureAssignmentHasNoConflictsAsync(model.AutoId, model.DriverId, route.DepartureTime, route.ArrivalTime, route.Id);
            }

            _mapper.Map(model, route);
            await _auditLogService.LogActionAsync(AuditAction.RouteUpdated, route.Id);
            await _unitOfWork.SaveChangesAsync();

            await _autoStatusSyncService.SyncAutoStatusAsync(assignedAuto);
            if (previousAutoId != assignedAuto.Id)
            {
                await _autoStatusSyncService.SyncAutoStatusAsync(previousAutoId);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var route = await GetRouteOrThrowAsync(id);
            var routeAutoId = route.AutoId;

            _routeRepository.Delete(route);
            await _auditLogService.LogActionAsync(AuditAction.RouteDeleted, id);
            await _unitOfWork.SaveChangesAsync();

            await _autoStatusSyncService.SyncAutoStatusAsync(routeAutoId);
            await _unitOfWork.SaveChangesAsync();
        }

        private static double GetDrivingHours(IEnumerable<RouteDTO> routes)
        {
            var drivingHours = routes.Sum(route => (route.ArrivalTime - route.DepartureTime).TotalHours);
            return Math.Round(drivingHours, 1);
        }

        private async Task<Route> GetRouteOrThrowAsync(int id)
        {
            return await _routeRepository.GetItemBySpecAsync(new RouteSpecs.ById(id)) ?? throw NotFoundException.For<Route>(id);
        }

        private async Task<Auto> GetUsableAutoAsync(int autoId)
        {
            var auto = await _autoRepository.GetItemBySpecAsync(new AutoSpecs.ById(autoId)) ?? throw NotFoundException.For<Auto>(autoId);
            if (auto.Status == AutoStatus.UnderMaintenance)
            {
                throw new BusinessRuleException("Автомобіль на ремонті.");
            }
            return auto;
        }

        private async Task EnsureDriverExistsAsync(int driverId)
        {
            if (!await _driverRepository.AnyBySpecAsync(new DriverSpecs.ById(driverId)))
            {
                throw NotFoundException.For<Driver>(driverId);
            }
        }

        private async Task EnsureLocationsExistAsync(params int[] locationIds)
        {
            var distinctLocationIds = locationIds.Distinct().ToArray();
            var existingLocationIds = await _locationRepository.GetProjectedListBySpecAsync(
                new LocationSpecs.ExistingIdsByIds(distinctLocationIds));
            if (existingLocationIds.Count == distinctLocationIds.Length)
            {
                return;
            }

            var missingLocationIds = distinctLocationIds.Except(existingLocationIds);
            throw NotFoundException.For<Location>(string.Join(", ", missingLocationIds));
        }

        private async Task EnsureAssignmentHasNoConflictsAsync(int autoId, int driverId, DateTime departure, DateTime arrival, int? excludeRouteId = null)
        {
            var routeIntervalText = $"{departure.ToString(DisplayFormats.DateTime)}–{arrival.ToString(DisplayFormats.DateTime)}";

            var conflictingAutoRouteId = await _routeRepository.GetProjectedItemBySpecAsync(
                new RouteSpecs.ActiveIdOverlappingForAuto(autoId, departure, arrival, excludeRouteId));
            if (conflictingAutoRouteId.HasValue)
            {
                throw new AssignmentConflictException(
                    $"Автомобіль (ID {autoId}) вже призначено на маршрут #{conflictingAutoRouteId.Value}, який перетинається з інтервалом {routeIntervalText}.");
            }

            var conflictingDriverRouteId = await _routeRepository.GetProjectedItemBySpecAsync(
                new RouteSpecs.ActiveIdOverlappingForDriver(driverId, departure, arrival, excludeRouteId));
            if (conflictingDriverRouteId.HasValue)
            {
                throw new AssignmentConflictException(
                    $"Водія (ID {driverId}) вже призначено на маршрут #{conflictingDriverRouteId.Value}, який перетинається з інтервалом {routeIntervalText}.");
            }

            var conflictingMaintenanceId = await _maintenanceRepository.GetProjectedItemBySpecAsync(
                new AutoMaintenanceSpecs.IncompleteIdOverlappingRouteIntervalForAuto(autoId, departure, arrival));
            if (conflictingMaintenanceId.HasValue)
            {
                throw new AssignmentConflictException(
                    $"Для автомобіля (ID {autoId}) заплановано техобслуговування #{conflictingMaintenanceId.Value}, яке припадає на інтервал {routeIntervalText}.");
            }
        }
    }
}
