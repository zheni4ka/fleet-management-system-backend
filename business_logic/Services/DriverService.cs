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
    public class DriverService : IDriverService
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Route> _routeRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateDriverRequestModel> _createValidator;
        private readonly IValidator<UpdateDriverRequestModel> _updateValidator;
        private readonly IValidator<GetPagedListRequestModel<DriverSortField>> _pagedListRequestValidator;

        public DriverService(
            IRepository<Driver> driverRepository,
            IRepository<Route> routeRepository,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateDriverRequestModel> createValidator,
            IValidator<UpdateDriverRequestModel> updateValidator,
            IValidator<GetPagedListRequestModel<DriverSortField>> pagedListRequestValidator)
        {
            _driverRepository = driverRepository;
            _routeRepository = routeRepository;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _pagedListRequestValidator = pagedListRequestValidator;
        }

        public async Task<GetPagedListResponseModel<GetDriverResponseModel>> GetPagedListAsync(GetPagedListRequestModel<DriverSortField> pagedListRequest)
        {
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var driversPagedListSpecification = new DriverSpecs.PagedList(pagedListRequest);
            var driversPage = await _driverRepository.GetProjectedListBySpecAsync(driversPagedListSpecification);
            var driversTotalCount = await _driverRepository.CountBySpecAsync(driversPagedListSpecification);

            return new GetPagedListResponseModel<GetDriverResponseModel>(
                Items: driversPage,
                TotalCount: driversTotalCount);
        }

        public async Task<IReadOnlyList<GetDriverLookupOptionResponseModel>> GetLookupOptionsAsync()
        {
            return await _driverRepository.GetProjectedListBySpecAsync(new DriverSpecs.LookupOptions());
        }

        public async Task<GetDriverResponseModel> GetAsync(int id)
        {
            var driver = await GetDriverOrThrowAsync(id);
            return _mapper.Map<GetDriverResponseModel>(driver);
        }

        public async Task<GetDriverResponseModel> CreateAsync(CreateDriverRequestModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);
            await EnsureLicenseIsUniqueAsync(model.LicenseNumber);

            var driver = _mapper.Map<Driver>(model);
            await using var driverCreationTransaction = await _unitOfWork.BeginTransactionAsync();
            await _driverRepository.InsertAsync(driver);
            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.DriverCreated, driver.Id);
            await _unitOfWork.SaveChangesAsync();
            await driverCreationTransaction.CommitAsync();

            return _mapper.Map<GetDriverResponseModel>(driver);
        }

        public async Task UpdateAsync(UpdateDriverRequestModel model)
        {
            await _updateValidator.ValidateAndThrowAsync(model);

            var driver = await GetDriverOrThrowAsync(model.Id);
            _driverRepository.SetExpectedRowVersion(driver, model.RowVersion);
            await EnsureLicenseIsUniqueAsync(model.LicenseNumber, model.Id);

            _mapper.Map(model, driver);
            await _auditLogService.LogActionAsync(AuditAction.DriverUpdated, driver.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var driver = await GetDriverOrThrowAsync(id);

            if (await _routeRepository.AnyBySpecAsync(new RouteSpecs.ByDriverId(id)))
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.DriverAssignedToRoutesTemplate, id));
            }

            _driverRepository.Delete(driver);
            await _auditLogService.LogActionAsync(AuditAction.DriverDeleted, id);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Driver> GetDriverOrThrowAsync(int id)
        {
            return await _driverRepository.GetItemBySpecAsync(new DriverSpecs.ById(id)) ?? throw NotFoundException.For<Driver>(id);
        }

        private async Task EnsureLicenseIsUniqueAsync(string licenseNumber, int? excludedDriverId = null)
        {
            if (await _driverRepository.AnyBySpecAsync(new DriverSpecs.ByLicenseNumber(licenseNumber, excludedDriverId)))
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.DriverLicenseNumberAlreadyExistsTemplate, licenseNumber));
            }
        }
    }
}
