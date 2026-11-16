using AutoMapper;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic.Specifications;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<LoginRequestModel> _loginValidator;
        private readonly IValidator<RegisterAccountRequestModel> _registerValidator;
        private readonly IRepository<IdentityRole> _roleRepository;
        private readonly IRepository<IdentityUser> _userRepository;
        private readonly IRepository<IdentityUserRole<string>> _userRoleRepository;
        private readonly IValidator<GetPagedListRequestModel<DispatcherSortField>> _dispatchersPagedListRequestValidator;

        public AccountService(
            UserManager<IdentityUser> userManager,
            ITokenService tokenService,
            ICurrentUserService currentUser,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<LoginRequestModel> loginValidator,
            IValidator<RegisterAccountRequestModel> registerValidator,
            IRepository<IdentityRole> roleRepository,
            IRepository<IdentityUser> userRepository,
            IRepository<IdentityUserRole<string>> userRoleRepository,
            IValidator<GetPagedListRequestModel<DispatcherSortField>> dispatchersPagedListRequestValidator)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _currentUser = currentUser;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _dispatchersPagedListRequestValidator = dispatchersPagedListRequestValidator;
        }

        public async Task<GetPagedListResponseModel<GetDispatcherResponseModel>> GetDispatchersPagedListAsync(GetPagedListRequestModel<DispatcherSortField> pagedListRequest)
        {
            await _dispatchersPagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var dispatcherRoleIdsQuery = _roleRepository.GetProjectedQueryBySpec(
                new IdentityRoleSpecs.IdsByName(UserRoles.Dispatcher));
            var dispatcherUserIdsQuery = _userRoleRepository.GetProjectedQueryBySpec(
                new IdentityUserRoleSpecs.UserIdsByRoleIds(dispatcherRoleIdsQuery));

            var dispatchersPagedListSpecification = new IdentityUserSpecs.DispatchersPagedList(dispatcherUserIdsQuery, pagedListRequest);
            var dispatchersPage = await _userRepository.GetProjectedListBySpecAsync(dispatchersPagedListSpecification);
            var dispatchersTotalCount = await _userRepository.CountBySpecAsync(dispatchersPagedListSpecification);

            return new GetPagedListResponseModel<GetDispatcherResponseModel>(
                Items: dispatchersPage,
                TotalCount: dispatchersTotalCount);
        }

        public async Task<LoginResponseModel> LoginAsync(LoginRequestModel model)
        {
            await _loginValidator.ValidateAndThrowAsync(model);

            var user = await _userManager.FindByNameAsync(model.Username)
                       ?? await _userManager.FindByEmailAsync(model.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                throw new UnauthorizedAccessException("Wrong username or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new LoginResponseModel(
                Token: _tokenService.CreateToken(user, roles),
                Username: user.UserName,
                Email: user.Email,
                Roles: roles.ToList());
        }

        public async Task<GetDispatcherResponseModel> RegisterAsync(RegisterAccountRequestModel model)
        {
            await _registerValidator.ValidateAndThrowAsync(model);

            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                throw new BusinessRuleException("User already exists");
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                throw new BusinessRuleException("Email already exists");
            }

            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true
            };

            EnsureSucceeded(await _userManager.CreateAsync(user, model.Password), "Failed to create user");
            EnsureSucceeded(await _userManager.AddToRoleAsync(user, model.Role), "Failed to assign role");

            await _auditLogService.LogActionAsync(AuditAction.UserRegistered, user.Id);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GetDispatcherResponseModel>(user);
        }

        public async Task<GetDispatcherResponseModel> GetDispatcherByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, UserRoles.Dispatcher))
            {
                throw new NotFoundException($"Dispatcher with id '{id}' was not found.");
            }

            return _mapper.Map<GetDispatcherResponseModel>(user);
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw new NotFoundException("User not found");

            if (_currentUser.UserId == id)
            {
                throw new BusinessRuleException("You cannot delete your own account");
            }

            EnsureSucceeded(await _userManager.DeleteAsync(user), "Error deleting user");

            await _auditLogService.LogActionAsync(AuditAction.UserDeleted, id);
            await _unitOfWork.SaveChangesAsync();
        }

        private static void EnsureSucceeded(IdentityResult result, string message)
        {
            if (!result.Succeeded)
            {
                var details = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BusinessRuleException($"{message}: {details}");
            }
        }
    }
}
