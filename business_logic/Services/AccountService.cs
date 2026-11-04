using AutoMapper;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
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
        private readonly IValidator<LoginModel> _loginValidator;
        private readonly IValidator<RegisterModel> _registerValidator;

        public AccountService(
            UserManager<IdentityUser> userManager,
            ITokenService tokenService,
            ICurrentUserService currentUser,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<LoginModel> loginValidator,
            IValidator<RegisterModel> registerValidator)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _currentUser = currentUser;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
        }

        public async Task<ResponseAuth> LoginAsync(LoginModel model)
        {
            await _loginValidator.ValidateAndThrowAsync(model);

            var user = await _userManager.FindByNameAsync(model.Username)
                       ?? await _userManager.FindByEmailAsync(model.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                throw new UnauthorizedAccessException("Wrong username or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new ResponseAuth
            {
                Token = _tokenService.CreateToken(user, roles),
                Username = user.UserName,
                Email = user.Email,
                Roles = roles
            };
        }

        public async Task<UserDTO> RegisterAsync(RegisterModel model)
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

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IEnumerable<UserDTO>> GetDispatchersAsync()
        {
            var dispatchers = await _userManager.GetUsersInRoleAsync(UserRoles.Dispatcher);
            return _mapper.Map<IEnumerable<UserDTO>>(dispatchers);
        }

        public async Task<UserDTO> GetDispatcherByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, UserRoles.Dispatcher))
            {
                throw new NotFoundException($"Dispatcher with id '{id}' was not found.");
            }

            return _mapper.Map<UserDTO>(user);
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
