using AutoMapper;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Helpers;
using static business_logic.Helpers.IdentityOperationResultGuard;
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
        private readonly ISessionResponseFactory _sessionResponseFactory;
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
        private readonly IRepository<UserDepot> _userDepotRepository;
        private readonly IDepotService _depotService;
        private readonly IValidator<UpdateUserDepotsRequestModel> _updateUserDepotsValidator;
        private readonly IValidator<UpdatePreferredLanguageRequestModel> _updatePreferredLanguageValidator;
        private readonly IValidator<GetPagedListRequestModel<DispatcherSortField>> _dispatchersPagedListRequestValidator;

        public AccountService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ISessionResponseFactory sessionResponseFactory,
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
            IRepository<UserDepot> userDepotRepository,
            IDepotService depotService,
            IValidator<UpdateUserDepotsRequestModel> updateUserDepotsValidator,
            IValidator<UpdatePreferredLanguageRequestModel> updatePreferredLanguageValidator,
            IValidator<GetPagedListRequestModel<DispatcherSortField>> dispatchersPagedListRequestValidator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _sessionResponseFactory = sessionResponseFactory;
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
            _userDepotRepository = userDepotRepository;
            _depotService = depotService;
            _updateUserDepotsValidator = updateUserDepotsValidator;
            _updatePreferredLanguageValidator = updatePreferredLanguageValidator;
            _dispatchersPagedListRequestValidator = dispatchersPagedListRequestValidator;
        }

        public async Task<GetPagedListResponseModel<GetDispatcherResponseModel>> GetDispatchersPagedListAsync(GetPagedListRequestModel<DispatcherSortField> pagedListRequest)
        {
            await _dispatchersPagedListRequestValidator.ValidateAndThrowAsync(pagedListRequest);

            var dispatchersPagedListSpecification = new IdentityUserSpecs.DispatchersPagedList(
                CreateDispatcherUserIdsQuery(),
                CreateUserDepotsQuery(),
                pagedListRequest);
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
            return await _sessionResponseFactory.CreateSessionResponseModelAsync<LoginResponseModel>(user, issuedRefreshToken);
        }

        public async Task<RefreshSessionResponseModel> RefreshSessionAsync(RefreshSessionRequestModel model)
        {
            await _refreshSessionValidator.ValidateAndThrowAsync(model);

            var rotatedRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(model.RefreshToken);
            var user = await _userManager.FindByIdAsync(rotatedRefreshToken.UserId)
                ?? throw new UnauthorizedAccessException(ExceptionMessages.RefreshTokenInvalid);

            return await _sessionResponseFactory.CreateSessionResponseModelAsync<RefreshSessionResponseModel>(user, rotatedRefreshToken.IssuedRefreshToken);
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

            return await _sessionResponseFactory.CreateSessionResponseModelAsync<ChangePasswordResponseModel>(user, issuedRefreshToken);
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
            if (!IsUserDeactivated(user))
            {
                EnsureIdentityOperationSucceeded(
                    await _userManager.SetLockoutEndDateAsync(user, null),
                    identityErrorDescriptions => string.Format(ExceptionMessages.PasswordResetFailedTemplate, model.UserId, identityErrorDescriptions));
            }
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

            await _depotService.EnsureDepotsExistAsync(model.DepotIds);

            await using var accountRegistrationTransaction = await _unitOfWork.BeginTransactionAsync();
            EnsureIdentityOperationSucceeded(
                await _userManager.CreateAsync(user, model.Password),
                identityErrorDescriptions => string.Format(ExceptionMessages.UserCreationFailedTemplate, model.Username, identityErrorDescriptions));
            EnsureIdentityOperationSucceeded(
                await _userManager.AddToRoleAsync(user, model.Role),
                identityErrorDescriptions => string.Format(ExceptionMessages.RoleAssignmentFailedTemplate, model.Role, model.Username, identityErrorDescriptions));
            foreach (var depotId in model.DepotIds.Distinct())
            {
                await _userDepotRepository.InsertAsync(new UserDepot { UserId = user.Id, DepotId = depotId });
            }

            await _auditLogService.LogActionAsync(AuditAction.UserRegistered, user.Id);
            await _unitOfWork.SaveChangesAsync();
            await accountRegistrationTransaction.CommitAsync();

            return _mapper.Map<GetDispatcherResponseModel>(user) with { DepotIds = model.DepotIds.Distinct().Order().ToList() };
        }

        public async Task<GetDispatcherResponseModel> GetDispatcherByIdAsync(string id)
        {
            return await _userRepository.GetProjectedItemBySpecAsync(
                new IdentityUserSpecs.DispatcherById(id, CreateDispatcherUserIdsQuery(), CreateUserDepotsQuery()))
                ?? throw NotFoundException.For<IdentityUser>(id);
        }

        public async Task UpdateUserDepotsAsync(UpdateUserDepotsRequestModel model)
        {
            await _updateUserDepotsValidator.ValidateAndThrowAsync(model);
            await GetDispatcherByIdAsync(model.UserId);
            await _depotService.EnsureDepotsExistAsync(model.DepotIds);

            await using var userDepotsUpdateTransaction = await _unitOfWork.BeginTransactionAsync();
            foreach (var removedUserDepot in await _userDepotRepository.GetListBySpecAsync(new UserDepotSpecs.ByUserId(model.UserId)))
            {
                _userDepotRepository.Delete(removedUserDepot);
            }
            await _unitOfWork.SaveChangesAsync();

            foreach (var depotId in model.DepotIds.Distinct())
            {
                await _userDepotRepository.InsertAsync(new UserDepot { UserId = model.UserId, DepotId = depotId });
            }

            await _auditLogService.LogActionAsync(AuditAction.UserDepotsUpdated, model.UserId);
            await _unitOfWork.SaveChangesAsync();
            await userDepotsUpdateTransaction.CommitAsync();
        }

        public async Task DeactivateUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw NotFoundException.For<IdentityUser>(id);

            if (_currentUser.UserId == id)
            {
                throw new BusinessRuleException(ExceptionMessages.OwnAccountDeactivationForbidden);
            }

            await using var userDeactivationTransaction = await _unitOfWork.BeginTransactionAsync();
            EnsureIdentityOperationSucceeded(
                await _userManager.SetLockoutEndDateAsync(user, AccountConstraints.DeactivatedUserLockoutEnd),
                identityErrorDescriptions => string.Format(ExceptionMessages.UserDeactivationFailedTemplate, id, identityErrorDescriptions));
            await _refreshTokenService.RevokeAllUserRefreshTokensAsync(id, RefreshTokenRevocationReason.UserDeactivated);

            await _auditLogService.LogActionAsync(AuditAction.UserDeactivated, id);
            await _unitOfWork.SaveChangesAsync();
            await userDeactivationTransaction.CommitAsync();
        }

        public async Task ReactivateUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id) ?? throw NotFoundException.For<IdentityUser>(id);
            if (!IsUserDeactivated(user))
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.UserNotDeactivatedTemplate, id));
            }

            await using var userReactivationTransaction = await _unitOfWork.BeginTransactionAsync();
            EnsureIdentityOperationSucceeded(
                await _userManager.SetLockoutEndDateAsync(user, null),
                identityErrorDescriptions => string.Format(ExceptionMessages.UserDeactivationFailedTemplate, id, identityErrorDescriptions));
            EnsureIdentityOperationSucceeded(
                await _userManager.ResetAccessFailedCountAsync(user),
                identityErrorDescriptions => string.Format(ExceptionMessages.UserDeactivationFailedTemplate, id, identityErrorDescriptions));

            await _auditLogService.LogActionAsync(AuditAction.UserReactivated, id);
            await _unitOfWork.SaveChangesAsync();
            await userReactivationTransaction.CommitAsync();
        }

        public async Task UpdatePreferredLanguageAsync(UpdatePreferredLanguageRequestModel model)
        {
            await _updatePreferredLanguageValidator.ValidateAndThrowAsync(model);
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException(ExceptionMessages.RefreshTokenInvalid);
            var user = await _userManager.FindByIdAsync(currentUserId) ?? throw NotFoundException.For<IdentityUser>(currentUserId);

            var preferredLanguageClaims = (await _userManager.GetClaimsAsync(user))
                .Where(userClaim => userClaim.Type == AccountClaimTypes.PreferredLanguage)
                .ToList();
            if (preferredLanguageClaims.Count > 0)
            {
                EnsureIdentityOperationSucceeded(
                    await _userManager.RemoveClaimsAsync(user, preferredLanguageClaims),
                    identityErrorDescriptions => string.Format(ExceptionMessages.PreferredLanguageUpdateFailedTemplate, currentUserId, identityErrorDescriptions));
            }

            EnsureIdentityOperationSucceeded(
                await _userManager.AddClaimAsync(user, new Claim(AccountClaimTypes.PreferredLanguage, model.Language!)),
                identityErrorDescriptions => string.Format(ExceptionMessages.PreferredLanguageUpdateFailedTemplate, currentUserId, identityErrorDescriptions));
        }

        private static bool IsUserDeactivated(IdentityUser user)
        {
            return user.LockoutEnd == AccountConstraints.DeactivatedUserLockoutEnd;
        }

        private IQueryable<string> CreateDispatcherUserIdsQuery()
        {
            var dispatcherRoleIdsQuery = _roleRepository.GetProjectedQueryBySpec(new IdentityRoleSpecs.IdsByName(UserRoles.Dispatcher));
            return _userRoleRepository.GetProjectedQueryBySpec(new IdentityUserRoleSpecs.UserIdsByRoleIds(dispatcherRoleIdsQuery));
        }

        private IQueryable<UserDepot> CreateUserDepotsQuery()
        {
            return _userDepotRepository.GetProjectedQueryBySpec(new UserDepotSpecs.AllUserDepots());
        }

        private async Task<IdentityUser> GetUserWithCheckedPasswordAsync(string usernameOrEmail, string password)
        {
            var user = await _userManager.FindByNameAsync(usernameOrEmail)
                       ?? await _userManager.FindByEmailAsync(usernameOrEmail)
                       ?? throw new UnauthorizedAccessException(ExceptionMessages.WrongUsernameOrPassword);

            var passwordSignInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (passwordSignInResult.IsLockedOut)
            {
                throw new UnauthorizedAccessException(IsUserDeactivated(user)
                    ? ExceptionMessages.AccountDeactivated
                    : ExceptionMessages.AccountLockedOut);
            }

            if (!passwordSignInResult.Succeeded)
            {
                throw new UnauthorizedAccessException(ExceptionMessages.WrongUsernameOrPassword);
            }

            return user;
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
    }
}
