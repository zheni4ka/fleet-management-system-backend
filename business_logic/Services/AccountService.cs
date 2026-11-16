using AutoMapper;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Helpers;
using business_logic.Interfaces;
using business_logic.Specifications;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace business_logic.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<LoginRequestModel> _loginValidator;
        private readonly IValidator<RegisterAccountRequestModel> _registerValidator;
        private readonly IValidator<RefreshSessionRequestModel> _refreshSessionValidator;
        private readonly IValidator<LogoutRequestModel> _logoutValidator;
        private readonly IValidator<ChangePasswordRequestModel> _changePasswordValidator;
        private readonly IValidator<ResetUserPasswordRequestModel> _resetUserPasswordValidator;
        private readonly IRepository<IdentityRole> _roleRepository;
        private readonly IRepository<IdentityUser> _userRepository;
        private readonly IRepository<IdentityUserRole<string>> _userRoleRepository;
        private readonly IValidator<GetPagedListRequestModel<DispatcherSortField>> _dispatchersPagedListRequestValidator;

        public AccountService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService,
            ICurrentUserService currentUser,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<LoginRequestModel> loginValidator,
            IValidator<RegisterAccountRequestModel> registerValidator,
            IValidator<RefreshSessionRequestModel> refreshSessionValidator,
            IValidator<LogoutRequestModel> logoutValidator,
            IValidator<ChangePasswordRequestModel> changePasswordValidator,
            IValidator<ResetUserPasswordRequestModel> resetUserPasswordValidator,
            IRepository<IdentityRole> roleRepository,
            IRepository<IdentityUser> userRepository,
            IRepository<IdentityUserRole<string>> userRoleRepository,
            IValidator<GetPagedListRequestModel<DispatcherSortField>> dispatchersPagedListRequestValidator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _currentUser = currentUser;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _refreshSessionValidator = refreshSessionValidator;
            _logoutValidator = logoutValidator;
            _changePasswordValidator = changePasswordValidator;
            _resetUserPasswordValidator = resetUserPasswordValidator;
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

            var user = await GetUserWithCheckedPasswordAsync(model.Username, model.Password);
            if (await IsPasswordChangeRequiredAsync(user))
            {
                throw new PasswordChangeRequiredException(ExceptionMessages.PasswordChangeRequired);
            }

            var issuedRefreshToken = await _refreshTokenService.IssueRefreshTokenInNewFamilyAsync(user.Id);
            return await CreateSessionResponseModelAsync<LoginResponseModel>(user, issuedRefreshToken);
        }

        public async Task<RefreshSessionResponseModel> RefreshSessionAsync(RefreshSessionRequestModel model)
        {
            await _refreshSessionValidator.ValidateAndThrowAsync(model);

            var rotatedRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(model.RefreshToken);
            var user = await _userManager.FindByIdAsync(rotatedRefreshToken.UserId)
                ?? throw new UnauthorizedAccessException(ExceptionMessages.RefreshTokenInvalid);

            return await CreateSessionResponseModelAsync<RefreshSessionResponseModel>(user, rotatedRefreshToken.IssuedRefreshToken);
        }

        public async Task LogoutAsync(LogoutRequestModel model)
        {
            await _logoutValidator.ValidateAndThrowAsync(model);
            await _refreshTokenService.RevokeRefreshTokenFamilyAsync(model.RefreshToken, RefreshTokenRevocationReason.Logout);
        }

        public async Task<ChangePasswordResponseModel> ChangePasswordAsync(ChangePasswordRequestModel model)
        {
            await _changePasswordValidator.ValidateAndThrowAsync(model);

            var user = await GetUserWithCheckedPasswordAsync(model.Username, model.CurrentPassword);

            await using var passwordChangeTransaction = await _unitOfWork.BeginTransactionAsync();
            EnsureIdentityOperationSucceeded(
                await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword),
                identityErrorDescriptions => string.Format(ExceptionMessages.PasswordChangeFailedTemplate, model.Username, identityErrorDescriptions));
            await RemovePasswordChangeRequirementAsync(user);
            await _refreshTokenService.RevokeAllUserRefreshTokensAsync(user.Id, RefreshTokenRevocationReason.PasswordChanged);

            await _auditLogService.LogActionAsync(AuditAction.UserPasswordChanged, user.Id);
            await _unitOfWork.SaveChangesAsync();
            var issuedRefreshToken = await _refreshTokenService.IssueRefreshTokenInNewFamilyAsync(user.Id);
            await passwordChangeTransaction.CommitAsync();

            return await CreateSessionResponseModelAsync<ChangePasswordResponseModel>(user, issuedRefreshToken);
        }

        public async Task<ResetUserPasswordResponseModel> ResetUserPasswordAsync(ResetUserPasswordRequestModel model)
        {
            await _resetUserPasswordValidator.ValidateAndThrowAsync(model);

            var user = await _userManager.FindByIdAsync(model.UserId) ?? throw NotFoundException.For<IdentityUser>(model.UserId);
            var temporaryPassword = TemporaryPasswordGenerator.GenerateTemporaryPassword();

            await using var passwordResetTransaction = await _unitOfWork.BeginTransactionAsync();
            EnsureIdentityOperationSucceeded(
                await _userManager.RemovePasswordAsync(user),
                identityErrorDescriptions => string.Format(ExceptionMessages.PasswordResetFailedTemplate, model.UserId, identityErrorDescriptions));
            EnsureIdentityOperationSucceeded(
                await _userManager.AddPasswordAsync(user, temporaryPassword),
                identityErrorDescriptions => string.Format(ExceptionMessages.PasswordResetFailedTemplate, model.UserId, identityErrorDescriptions));
            EnsureIdentityOperationSucceeded(
                await _userManager.ResetAccessFailedCountAsync(user),
                identityErrorDescriptions => string.Format(ExceptionMessages.PasswordResetFailedTemplate, model.UserId, identityErrorDescriptions));
            EnsureIdentityOperationSucceeded(
                await _userManager.SetLockoutEndDateAsync(user, null),
                identityErrorDescriptions => string.Format(ExceptionMessages.PasswordResetFailedTemplate, model.UserId, identityErrorDescriptions));
            await AddPasswordChangeRequirementAsync(user);
            await _refreshTokenService.RevokeAllUserRefreshTokensAsync(user.Id, RefreshTokenRevocationReason.PasswordReset);

            await _auditLogService.LogActionAsync(AuditAction.UserPasswordReset, user.Id);
            await _unitOfWork.SaveChangesAsync();
            await passwordResetTransaction.CommitAsync();

            return new ResetUserPasswordResponseModel(TemporaryPassword: temporaryPassword);
        }

        public async Task<GetDispatcherResponseModel> RegisterAsync(RegisterAccountRequestModel model)
        {
            await _registerValidator.ValidateAndThrowAsync(model);

            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.UsernameAlreadyExistsTemplate, model.Username));
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.EmailAlreadyExistsTemplate, model.Email));
            }

            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email,
                EmailConfirmed = true
            };

            await using var accountRegistrationTransaction = await _unitOfWork.BeginTransactionAsync();
            EnsureIdentityOperationSucceeded(
                await _userManager.CreateAsync(user, model.Password),
                identityErrorDescriptions => string.Format(ExceptionMessages.UserCreationFailedTemplate, model.Username, identityErrorDescriptions));
            EnsureIdentityOperationSucceeded(
                await _userManager.AddToRoleAsync(user, model.Role),
                identityErrorDescriptions => string.Format(ExceptionMessages.RoleAssignmentFailedTemplate, model.Role, model.Username, identityErrorDescriptions));

            await _auditLogService.LogActionAsync(AuditAction.UserRegistered, user.Id);
            await _unitOfWork.SaveChangesAsync();
            await accountRegistrationTransaction.CommitAsync();

            return _mapper.Map<GetDispatcherResponseModel>(user);
        }

        public async Task<GetDispatcherResponseModel> GetDispatcherByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !await _userManager.IsInRoleAsync(user, UserRoles.Dispatcher))
            {
                throw NotFoundException.For<IdentityUser>(id);
            }

            return _mapper.Map<GetDispatcherResponseModel>(user);
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw NotFoundException.For<IdentityUser>(id);

            if (_currentUser.UserId == id)
            {
                throw new BusinessRuleException(ExceptionMessages.OwnAccountDeletionForbidden);
            }

            await using var userDeletionTransaction = await _unitOfWork.BeginTransactionAsync();
            EnsureIdentityOperationSucceeded(
                await _userManager.DeleteAsync(user),
                identityErrorDescriptions => string.Format(ExceptionMessages.UserDeletionFailedTemplate, id, identityErrorDescriptions));

            await _auditLogService.LogActionAsync(AuditAction.UserDeleted, id);
            await _unitOfWork.SaveChangesAsync();
            await userDeletionTransaction.CommitAsync();
        }

        private async Task<IdentityUser> GetUserWithCheckedPasswordAsync(string usernameOrEmail, string password)
        {
            var user = await _userManager.FindByNameAsync(usernameOrEmail)
                       ?? await _userManager.FindByEmailAsync(usernameOrEmail)
                       ?? throw new UnauthorizedAccessException(ExceptionMessages.WrongUsernameOrPassword);

            var passwordSignInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (passwordSignInResult.IsLockedOut)
            {
                throw new UnauthorizedAccessException(ExceptionMessages.AccountLockedOut);
            }

            if (!passwordSignInResult.Succeeded)
            {
                throw new UnauthorizedAccessException(ExceptionMessages.WrongUsernameOrPassword);
            }

            return user;
        }

        private async Task<TSessionResponseModel> CreateSessionResponseModelAsync<TSessionResponseModel>(
            IdentityUser user,
            IssuedRefreshTokenModel issuedRefreshToken)
            where TSessionResponseModel : SessionResponseModelBase, new()
        {
            var roles = await _userManager.GetRolesAsync(user);
            var issuedAccessToken = _tokenService.CreateAccessToken(user, roles);

            return new TSessionResponseModel
            {
                AccessToken = issuedAccessToken.AccessToken,
                AccessTokenExpiresAtUtc = issuedAccessToken.ExpiresAtUtc,
                RefreshToken = issuedRefreshToken.RefreshToken,
                RefreshTokenExpiresAtUtc = issuedRefreshToken.ExpiresAtUtc,
                Username = user.UserName!,
                Email = user.Email,
                Roles = roles.ToList()
            };
        }

        private async Task<bool> IsPasswordChangeRequiredAsync(IdentityUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            return userClaims.Any(userClaim => userClaim.Type == AccountClaimTypes.IsPasswordChangeRequired);
        }

        private async Task AddPasswordChangeRequirementAsync(IdentityUser user)
        {
            if (await IsPasswordChangeRequiredAsync(user))
            {
                return;
            }

            EnsureIdentityOperationSucceeded(
                await _userManager.AddClaimAsync(user, new Claim(AccountClaimTypes.IsPasswordChangeRequired, AccountClaimTypes.IsPasswordChangeRequiredValue)),
                identityErrorDescriptions => string.Format(ExceptionMessages.PasswordResetFailedTemplate, user.Id, identityErrorDescriptions));
        }

        private async Task RemovePasswordChangeRequirementAsync(IdentityUser user)
        {
            var passwordChangeRequirementClaims = (await _userManager.GetClaimsAsync(user))
                .Where(userClaim => userClaim.Type == AccountClaimTypes.IsPasswordChangeRequired)
                .ToList();
            if (passwordChangeRequirementClaims.Count == 0)
            {
                return;
            }

            EnsureIdentityOperationSucceeded(
                await _userManager.RemoveClaimsAsync(user, passwordChangeRequirementClaims),
                identityErrorDescriptions => string.Format(ExceptionMessages.PasswordChangeFailedTemplate, user.UserName, identityErrorDescriptions));
        }

        private static void EnsureIdentityOperationSucceeded(IdentityResult identityOperationResult, Func<string, string> createFailureMessage)
        {
            if (identityOperationResult.Succeeded)
            {
                return;
            }

            var identityErrorDescriptions = string.Join("; ", identityOperationResult.Errors.Select(identityError => identityError.Description));
            throw new BusinessRuleException(createFailureMessage(identityErrorDescriptions));
        }
    }
}
