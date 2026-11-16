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
        private readonly IValidator<CreateAutoRequestModel> _createValidator;
        private readonly IValidator<UpdateAutoRequestModel> _editValidator;
        private readonly IValidator<GetPagedListRequestModel<AutoSortField>> _pagedListRequestValidator;
        private readonly IValidator<GetAutosBySearchFilterRequestModel> _searchFilterValidator;

        public AutoService(
            IRepository<Auto> autoRepository,
            IRepository<Route> routeRepository,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateAutoRequestModel> createValidator,
            IValidator<UpdateAutoRequestModel> editValidator,
            IValidator<GetPagedListRequestModel<AutoSortField>> pagedListRequestValidator,
            IValidator<GetAutosBySearchFilterRequestModel> searchFilterValidator)
        {
            _autoRepository = autoRepository;
            _routeRepository = routeRepository;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _editValidator = editValidator;
            _pagedListRequestValidator = pagedListRequestValidator;
            _searchFilterValidator = searchFilterValidator;
        }

        public async Task<GetPagedListResponseModel<GetAutoResponseModel>> GetPagedListAsync(
            GetAutosBySearchFilterRequestModel searchFilter,
            GetPagedListRequestModel<AutoSortField> pagedListRequest)
        {
            await _searchFilterValidator.ValidateAndThrowAsync(searchFilter);
            await _pagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var autosPagedListSpecification = new AutoSpecs.PagedList(searchFilter, pagedListRequest);
            var autosPage = await _autoRepository.GetProjectedListBySpecAsync(autosPagedListSpecification);
            var autosTotalCount = await _autoRepository.CountBySpecAsync(autosPagedListSpecification);

            return new GetPagedListResponseModel<GetAutoResponseModel>(
                Items: autosPage,
                TotalCount: autosTotalCount);
        }

        public async Task<IReadOnlyList<GetAutoLookupOptionResponseModel>> GetLookupOptionsAsync()
        {
            return await _autoRepository.GetProjectedListBySpecAsync(new AutoSpecs.LookupOptions());
        }

        public async Task<GetAutoResponseModel> GetAsync(int id)
        {
            var auto = await GetAutoOrThrowAsync(id);
            return _mapper.Map<GetAutoResponseModel>(auto);
        }

        public async Task<GetAutoResponseModel> CreateAsync(CreateAutoRequestModel model)
        {
            await _createValidator.ValidateAndThrowAsync(model);
            await EnsureNumberIsUniqueAsync(model.Number);

            var auto = _mapper.Map<Auto>(model);
            await _autoRepository.InsertAsync(auto);
            await _unitOfWork.SaveChangesAsync();
            await _auditLogService.LogActionAsync(AuditAction.AutoCreated, auto.Id);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GetAutoResponseModel>(auto);
        }

        public async Task UpdateAsync(UpdateAutoRequestModel model)
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

        private async Task<Auto> GetAutoOrThrowAsync(int id)
        {
            return await _autoRepository.GetItemBySpecAsync(new AutoSpecs.ById(id)) ?? throw NotFoundException.For<Auto>(id);
        }

        private async Task EnsureNumberIsUniqueAsync(string number, int? excludedAutoId = null)
        {
            if (await _autoRepository.AnyBySpecAsync(new AutoSpecs.ByNumber(number, excludedAutoId)))
            {
                throw new BusinessRuleException($"Auto with number '{number}' already exists.");
            }
        }
    }
}
