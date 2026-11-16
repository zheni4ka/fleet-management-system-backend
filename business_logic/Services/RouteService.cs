using AutoMapper;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Helpers;
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
        private readonly IDatabaseApplicationLockService _databaseApplicationLockService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateRouteRequestModel> _createValidator;
        private readonly IValidator<UpdateRouteRequestModel> _updateValidator;
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
            IDatabaseApplicationLockService databaseApplicationLockService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateRouteRequestModel> createValidator,
            IValidator<UpdateRouteRequestModel> updateValidator,
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
            _databaseApplicationLockService = databaseApplicationLockService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _searchFilterValidator = searchFilterValidator;
            _periodReportValidator = periodReportValidator;
            _pagedListRequestValidator = pagedListRequestValidator;
        }

        public async Task<GetRouteResponseModel> GetAsync(int id)
        {
            return await _routeRepository.GetProjectedItemBySpecAsync(new RouteSpecs.ResponseModelById(id))
                ?? throw NotFoundException.For<Route>(id);
        }

        public async Task<GetPagedListResponseModel<GetRouteResponseModel>> GetRoutesPagedListBySearchFilterAsync(
            GetRoutesBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<RouteSortField> pagedListRequest)
        {
            await _searchFilterValidator.ValidateAndThrowAsync(searchFilter);
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var routesPagedListSpecification = new RouteSpecs.PagedListBySearchFilter(searchFilter, pagedListRequest);
            var routesPage = await _routeRepository.GetProjectedListBySpecAsync(routesPagedListSpecification);
            var routesTotalCount = await _routeRepository.CountBySpecAsync(routesPagedListSpecification);

            return new GetPagedListResponseModel<GetRouteResponseModel>(
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

        public async Task<GetRouteResponseModel> CreateAsync(CreateRouteRequestModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);

            var route = _mapper.Map<Route>(model);
            await using (var routeCreationTransaction = await _unitOfWork.BeginTransactionAsync())
            {
                await AcquireAssignmentLocksAsync(model.AutoId, model.DriverId);
                var auto = await GetUsableAutoAsync(model.AutoId);
                await EnsureDriverIsAssignableWithAutoAsync(model.DriverId, auto, isDriverNewlyAssigned: true);
                await EnsureLocationsAreAssignableAsync(model.StartLocationId, model.DestinationLocationId);
                route.DepotId = auto.DepotId;

                if (RouteStatusGroups.Active.Contains(model.Status))
                {
                    await EnsureAssignmentHasNoConflictsAsync(model.AutoId, model.DriverId, model.DepartureTime, model.ArrivalTime);
                }

                await _routeRepository.InsertAsync(route);
                await _unitOfWork.SaveChangesAsync();
                await _auditLogService.LogActionAsync(AuditAction.RouteCreated, route.Id);
                await _autoStatusSyncService.SyncAutoStatusAsync(auto);
                await _unitOfWork.SaveChangesAsync();
                await routeCreationTransaction.CommitAsync();
            }

            return await GetAsync(route.Id);
        }

        public async Task UpdateAsync(UpdateRouteRequestModel model)
        {
            await _updateValidator.ValidateAndThrowAsync(model);

            await using var routeUpdateTransaction = await _unitOfWork.BeginTransactionAsync();
            await AcquireAssignmentLocksAsync(model.AutoId, model.DriverId);
            var route = await GetRouteOrThrowAsync(model.Id);
            _routeRepository.SetExpectedRowVersion(route, model.RowVersion);
            var previousAutoId = route.AutoId;

            var assignedAuto = previousAutoId == model.AutoId
                ? await GetAutoOrThrowAsync(model.AutoId)
                : await GetUsableAutoAsync(model.AutoId);
            await EnsureDriverIsAssignableWithAutoAsync(model.DriverId, assignedAuto, isDriverNewlyAssigned: model.DriverId != route.DriverId);
            var currentRouteLocationIds = new[] { route.StartLocationId, route.DestinationLocationId };
            await EnsureLocationsAreAssignableAsync(
                new[] { model.StartLocationId, model.DestinationLocationId }.Except(currentRouteLocationIds).ToArray());

            if (RouteStatusGroups.Active.Contains(model.Status))
            {
                await EnsureAssignmentHasNoConflictsAsync(model.AutoId, model.DriverId, route.DepartureTime, route.ArrivalTime, route.Id);
            }

            _mapper.Map(model, route);
            route.DepotId = assignedAuto.DepotId;
            await _auditLogService.LogActionAsync(AuditAction.RouteUpdated, route.Id);
            await _unitOfWork.SaveChangesAsync();

            await _autoStatusSyncService.SyncAutoStatusAsync(assignedAuto);
            if (previousAutoId != assignedAuto.Id)
            {
                await _autoStatusSyncService.SyncAutoStatusAsync(previousAutoId);
            }
            await _unitOfWork.SaveChangesAsync();
            await routeUpdateTransaction.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var route = await GetRouteOrThrowAsync(id);
            var routeAutoId = route.AutoId;

            await using var routeDeletionTransaction = await _unitOfWork.BeginTransactionAsync();
            _routeRepository.Delete(route);
            await _auditLogService.LogActionAsync(AuditAction.RouteDeleted, id);
            await _unitOfWork.SaveChangesAsync();

            await _autoStatusSyncService.SyncAutoStatusAsync(routeAutoId);
            await _unitOfWork.SaveChangesAsync();
            await routeDeletionTransaction.CommitAsync();
        }

        private static double GetDrivingHours(IEnumerable<GetRouteResponseModel> routes)
        {
            var drivingHours = routes.Sum(route => (route.ArrivalTime - route.DepartureTime).TotalHours);
            return Math.Round(drivingHours, 1);
        }

        private Task AcquireAssignmentLocksAsync(int autoId, int driverId)
        {
            return _databaseApplicationLockService.AcquireTransactionExclusiveLocksAsync(
            [
                ApplicationLockResourceNameFactory.CreateAutoAssignmentLockResourceName(autoId),
                ApplicationLockResourceNameFactory.CreateDriverAssignmentLockResourceName(driverId)
            ]);
        }

        private async Task<Route> GetRouteOrThrowAsync(int id)
        {
            return await _routeRepository.GetItemBySpecAsync(new RouteSpecs.ById(id)) ?? throw NotFoundException.For<Route>(id);
        }

        private async Task<Auto> GetUsableAutoAsync(int autoId)
        {
            var auto = await GetAutoOrThrowAsync(autoId);
            if (auto.IsArchived)
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.AutoArchivedTemplate, autoId));
            }

            if (auto.Status == AutoStatus.UnderMaintenance)
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.AutoUnderMaintenanceTemplate, autoId));
            }
            return auto;
        }

        private async Task<Auto> GetAutoOrThrowAsync(int autoId)
        {
            return await _autoRepository.GetItemBySpecAsync(new AutoSpecs.ById(autoId)) ?? throw NotFoundException.For<Auto>(autoId);
        }

        private async Task EnsureDriverIsAssignableWithAutoAsync(int driverId, Auto assignedAuto, bool isDriverNewlyAssigned)
        {
            var driverAssignmentState = await _driverRepository.GetProjectedItemBySpecAsync(new DriverSpecs.AssignmentStateById(driverId))
                ?? throw NotFoundException.For<Driver>(driverId);
            if (isDriverNewlyAssigned && driverAssignmentState.IsArchived)
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.DriverArchivedTemplate, driverId));
            }

            if (driverAssignmentState.DepotId != assignedAuto.DepotId)
            {
                throw new BusinessRuleException(string.Format(
                    ExceptionMessages.DriverDepotMismatchTemplate, driverId, driverAssignmentState.DepotId, assignedAuto.Id, assignedAuto.DepotId));
            }
        }

        private async Task EnsureLocationsAreAssignableAsync(params int[] newlyAssignedLocationIds)
        {
            var distinctLocationIds = newlyAssignedLocationIds.Distinct().ToArray();
            if (distinctLocationIds.Length == 0)
            {
                return;
            }

            var locationArchiveStates = await _locationRepository.GetProjectedListBySpecAsync(
                new LocationSpecs.ArchiveStatesByIds(distinctLocationIds));
            var missingLocationIds = distinctLocationIds.Except(locationArchiveStates.Select(locationArchiveState => locationArchiveState.Id)).ToList();
            if (missingLocationIds.Count > 0)
            {
                throw NotFoundException.For<Location>(string.Join(", ", missingLocationIds));
            }

            var archivedLocationIds = locationArchiveStates
                .Where(locationArchiveState => locationArchiveState.IsArchived)
                .Select(locationArchiveState => locationArchiveState.Id)
                .ToList();
            if (archivedLocationIds.Count > 0)
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.LocationsArchivedTemplate, string.Join(", ", archivedLocationIds)));
            }
        }

        private async Task EnsureAssignmentHasNoConflictsAsync(int autoId, int driverId, DateTime departure, DateTime arrival, int? excludeRouteId = null)
        {
            var routeIntervalText = $"{departure.ToString(DisplayFormats.DateTime)}–{arrival.ToString(DisplayFormats.DateTime)}";

            var conflictingAutoRouteId = await _routeRepository.GetProjectedItemBySpecAsync(
                new RouteSpecs.ActiveIdOverlappingForAuto(autoId, departure, arrival, excludeRouteId));
            if (conflictingAutoRouteId.HasValue)
            {
                throw new AssignmentConflictException(string.Format(
                    ExceptionMessages.AutoAssignmentConflictTemplate, autoId, conflictingAutoRouteId.Value, routeIntervalText));
            }

            var conflictingDriverRouteId = await _routeRepository.GetProjectedItemBySpecAsync(
                new RouteSpecs.ActiveIdOverlappingForDriver(driverId, departure, arrival, excludeRouteId));
            if (conflictingDriverRouteId.HasValue)
            {
                throw new AssignmentConflictException(string.Format(
                    ExceptionMessages.DriverAssignmentConflictTemplate, driverId, conflictingDriverRouteId.Value, routeIntervalText));
            }

            var conflictingMaintenanceId = await _maintenanceRepository.GetProjectedItemBySpecAsync(
                new AutoMaintenanceSpecs.IncompleteIdOverlappingRouteIntervalForAuto(autoId, departure, arrival));
            if (conflictingMaintenanceId.HasValue)
            {
                throw new AssignmentConflictException(string.Format(
                    ExceptionMessages.MaintenanceAssignmentConflictTemplate, autoId, conflictingMaintenanceId.Value, routeIntervalText));
            }
        }
    }
}
