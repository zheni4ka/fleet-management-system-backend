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
    public class LocationService : ILocationService
    {
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Route> _routeRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateLocationRequestModel> _createValidator;
        private readonly IValidator<GetPagedListRequestModel<LocationSortField>> _pagedListRequestValidator;
        private readonly IValidator<GetLocationsBySearchFilterRequestModel> _searchFilterValidator;

        public LocationService(
            IRepository<Location> locationRepository,
            IRepository<Route> routeRepository,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateLocationRequestModel> createValidator,
            IValidator<GetPagedListRequestModel<LocationSortField>> pagedListRequestValidator,
            IValidator<GetLocationsBySearchFilterRequestModel> searchFilterValidator)
        {
            _locationRepository = locationRepository;
            _routeRepository = routeRepository;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _pagedListRequestValidator = pagedListRequestValidator;
            _searchFilterValidator = searchFilterValidator;
        }

        public async Task<GetPagedListResponseModel<GetLocationResponseModel>> GetPagedListAsync(
            GetLocationsBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<LocationSortField> pagedListRequest)
        {
            await _searchFilterValidator.ValidateAndThrowAsync(searchFilter);
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var locationsPagedListSpecification = new LocationSpecs.PagedList(searchFilter, pagedListRequest);
            var locationsPage = await _locationRepository.GetProjectedListBySpecAsync(locationsPagedListSpecification);
            var locationsTotalCount = await _locationRepository.CountBySpecAsync(locationsPagedListSpecification);

            return new GetPagedListResponseModel<GetLocationResponseModel>(
                Items: locationsPage,
                TotalCount: locationsTotalCount);
        }

        public async Task<IReadOnlyList<GetLocationLookupOptionResponseModel>> GetLookupOptionsAsync()
        {
            return await _locationRepository.GetProjectedListBySpecAsync(new LocationSpecs.LookupOptions());
        }

        public async Task<GetLocationResponseModel> GetAsync(int id)
        {
            var location = await GetLocationOrThrowAsync(id);
            return _mapper.Map<GetLocationResponseModel>(location);
        }

        public async Task<GetLocationResponseModel> CreateAsync(CreateLocationRequestModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);

            var location = _mapper.Map<Location>(model);
            await using var locationCreationTransaction = await _unitOfWork.BeginTransactionAsync();
            await _locationRepository.InsertAsync(location);
            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.LocationCreated, location.Id);
            await _unitOfWork.SaveChangesAsync();
            await locationCreationTransaction.CommitAsync();

            return _mapper.Map<GetLocationResponseModel>(location);
        }

        public async Task<DeleteLocationResponseModel> DeleteAsync(int id)
        {
            var location = await GetLocationOrThrowAsync(id);

            if (await _routeRepository.AnyBySpecAsync(new RouteSpecs.ActiveByLocationId(id)))
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.LocationHasActiveRoutesTemplate, id));
            }

            var isLocationReferencedByRoutes = await _routeRepository.AnyBySpecAsync(new RouteSpecs.ByLocationId(id));
            if (isLocationReferencedByRoutes)
            {
                ArchivingHelper.ArchiveEntity(location);
                await _auditLogService.LogActionAsync(AuditAction.LocationArchived, id);
            }
            else
            {
                _locationRepository.Delete(location);
                await _auditLogService.LogActionAsync(AuditAction.LocationDeleted, id);
            }

            await _unitOfWork.SaveChangesAsync();
            return new DeleteLocationResponseModel { IsArchivedInsteadOfDeleted = isLocationReferencedByRoutes };
        }

        public async Task RestoreAsync(int id)
        {
            var location = await GetLocationOrThrowAsync(id);
            ArchivingHelper.RestoreArchivedEntity(location, id);
            await _auditLogService.LogActionAsync(AuditAction.LocationRestored, id);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Location> GetLocationOrThrowAsync(int id)
        {
            return await _locationRepository.GetItemBySpecAsync(new LocationSpecs.ById(id)) ?? throw NotFoundException.For<Location>(id);
        }
    }
}
