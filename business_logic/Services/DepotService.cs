using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic.Specifications;
using FluentValidation;

namespace business_logic.Services
{
    public class DepotService : IDepotService
    {
        private readonly IRepository<Depot> _depotRepository;
        private readonly IRepository<Auto> _autoRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Route> _routeRepository;
        private readonly IRepository<UserDepot> _userDepotRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateDepotRequestModel> _createValidator;
        private readonly IValidator<UpdateDepotRequestModel> _updateValidator;

        public DepotService(
            IRepository<Depot> depotRepository,
            IRepository<Auto> autoRepository,
            IRepository<Driver> driverRepository,
            IRepository<Route> routeRepository,
            IRepository<UserDepot> userDepotRepository,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IValidator<CreateDepotRequestModel> createValidator,
            IValidator<UpdateDepotRequestModel> updateValidator)
        {
            _depotRepository = depotRepository;
            _autoRepository = autoRepository;
            _driverRepository = driverRepository;
            _routeRepository = routeRepository;
            _userDepotRepository = userDepotRepository;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IReadOnlyList<GetDepotResponseModel>> GetDepotsAsync()
        {
            return await _depotRepository.GetProjectedListBySpecAsync(new DepotSpecs.ResponseModels());
        }

        public async Task<GetDepotResponseModel> CreateAsync(CreateDepotRequestModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);
            var depotName = model.Name!.Trim();
            await EnsureDepotNameIsUniqueAsync(depotName);

            var depot = new Depot { Name = depotName };
            await using var depotCreationTransaction = await _unitOfWork.BeginTransactionAsync();
            await _depotRepository.InsertAsync(depot);
            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.DepotCreated, depot.Id);
            await _unitOfWork.SaveChangesAsync();
            await depotCreationTransaction.CommitAsync();

            return new GetDepotResponseModel { Id = depot.Id, Name = depot.Name };
        }

        public async Task UpdateAsync(UpdateDepotRequestModel model)
        {
            await _updateValidator.ValidateAndThrowAsync(model);
            var depotName = model.Name!.Trim();

            var depot = await GetDepotOrThrowAsync(model.Id);
            await EnsureDepotNameIsUniqueAsync(depotName, model.Id);

            depot.Name = depotName;
            await _auditLogService.LogActionAsync(AuditAction.DepotUpdated, depot.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var depot = await GetDepotOrThrowAsync(id);

            var isDepotInUse = await _autoRepository.AnyBySpecAsync(new AutoSpecs.ByDepotId(id))
                || await _driverRepository.AnyBySpecAsync(new DriverSpecs.ByDepotId(id))
                || await _routeRepository.AnyBySpecAsync(new RouteSpecs.ByDepotId(id))
                || await _userDepotRepository.AnyBySpecAsync(new UserDepotSpecs.ByDepotId(id));
            if (isDepotInUse)
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.DepotInUseTemplate, id));
            }

            _depotRepository.Delete(depot);
            await _auditLogService.LogActionAsync(AuditAction.DepotDeleted, id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task EnsureDepotsExistAsync(IReadOnlyCollection<int> depotIds)
        {
            var distinctDepotIds = depotIds.Distinct().ToArray();
            var existingDepotIds = await _depotRepository.GetProjectedListBySpecAsync(new DepotSpecs.ExistingIdsByIds(distinctDepotIds));
            if (existingDepotIds.Count == distinctDepotIds.Length)
            {
                return;
            }

            throw NotFoundException.For<Depot>(string.Join(", ", distinctDepotIds.Except(existingDepotIds)));
        }

        private async Task<Depot> GetDepotOrThrowAsync(int id)
        {
            return await _depotRepository.GetItemBySpecAsync(new DepotSpecs.ById(id)) ?? throw NotFoundException.For<Depot>(id);
        }

        private async Task EnsureDepotNameIsUniqueAsync(string depotName, int? excludedDepotId = null)
        {
            if (await _depotRepository.AnyBySpecAsync(new DepotSpecs.ByName(depotName, excludedDepotId)))
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.DepotNameAlreadyExistsTemplate, depotName));
            }
        }
    }
}
