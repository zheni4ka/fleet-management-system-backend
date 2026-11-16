using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using static business_logic.Helpers.IdentityOperationResultGuard;
using business_logic.Interfaces;
using business_logic.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace business_logic.Services
{
    public class ExternalLoginService : IExternalLoginService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IExternalIdentityTokenValidator _externalIdentityTokenValidator;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ISessionResponseFactory _sessionResponseFactory;
        private readonly IAuditLogService _auditLogService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<LoginWithExternalIdentityRequestModel> _loginWithExternalIdentityValidator;
        private readonly ExternalIdentitySettings _externalIdentitySettings;

        public ExternalLoginService(
            UserManager<IdentityUser> userManager,
            IExternalIdentityTokenValidator externalIdentityTokenValidator,
            IRefreshTokenService refreshTokenService,
            ISessionResponseFactory sessionResponseFactory,
            IAuditLogService auditLogService,
            IUnitOfWork unitOfWork,
            IValidator<LoginWithExternalIdentityRequestModel> loginWithExternalIdentityValidator,
            IOptions<ExternalIdentitySettings> externalIdentitySettingsOptions)
        {
            _userManager = userManager;
            _externalIdentityTokenValidator = externalIdentityTokenValidator;
            _refreshTokenService = refreshTokenService;
            _sessionResponseFactory = sessionResponseFactory;
            _auditLogService = auditLogService;
            _unitOfWork = unitOfWork;
            _loginWithExternalIdentityValidator = loginWithExternalIdentityValidator;
            _externalIdentitySettings = externalIdentitySettingsOptions.Value;
        }

        public async Task<LoginWithExternalIdentityResponseModel> LoginWithExternalIdentityAsync(LoginWithExternalIdentityRequestModel model)
        {
            await _loginWithExternalIdentityValidator.ValidateAndThrowAsync(model);

            var validatedExternalIdentity = await _externalIdentityTokenValidator.ValidateIdTokenAsync(model.IdToken, model.Nonce);
            var mappedRoleNames = _externalIdentitySettings.GroupRoleMappings
                .Where(groupRoleMapping => validatedExternalIdentity.GroupIds.Contains(groupRoleMapping.Key))
                .Select(groupRoleMapping => groupRoleMapping.Value)
                .Distinct()
                .ToList();
            if (mappedRoleNames.Count == 0)
            {
                throw new ExternalIdentityAccessDeniedException(
                    string.Format(ExceptionMessages.ExternalIdentityHasNoMappedGroupTemplate, validatedExternalIdentity.SubjectId));
            }

            await using var externalLoginTransaction = await _unitOfWork.BeginTransactionAsync();
            var user = await _userManager.FindByLoginAsync(ExternalLoginProviderNames.EntraId, validatedExternalIdentity.SubjectId)
                ?? await ProvisionExternalUserAsync(validatedExternalIdentity);

            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new UnauthorizedAccessException(ExceptionMessages.AccountLockedOut);
            }

            await SynchronizeUserRolesAsync(user, mappedRoleNames);
            await _unitOfWork.SaveChangesAsync();
            var issuedRefreshToken = await _refreshTokenService.IssueRefreshTokenInNewFamilyAsync(user.Id);
            await externalLoginTransaction.CommitAsync();

            return await _sessionResponseFactory.CreateSessionResponseModelAsync<LoginWithExternalIdentityResponseModel>(user, issuedRefreshToken);
        }

        private async Task<IdentityUser> ProvisionExternalUserAsync(ValidatedExternalIdentityModel validatedExternalIdentity)
        {
            var provisionedUsername = validatedExternalIdentity.PreferredUsername
                ?? validatedExternalIdentity.Email
                ?? throw new BusinessRuleException(
                    string.Format(ExceptionMessages.ExternalIdentityUsernameMissingTemplate, validatedExternalIdentity.SubjectId));

            if (await _userManager.FindByNameAsync(provisionedUsername) != null)
            {
                throw new BusinessRuleException(string.Format(ExceptionMessages.UsernameAlreadyExistsTemplate, provisionedUsername));
            }

            var provisionedUser = new IdentityUser
            {
                UserName = provisionedUsername,
                Email = validatedExternalIdentity.Email,
                EmailConfirmed = validatedExternalIdentity.Email != null
            };
            EnsureIdentityOperationSucceeded(
                await _userManager.CreateAsync(provisionedUser),
                identityErrorDescriptions => string.Format(ExceptionMessages.UserCreationFailedTemplate, provisionedUsername, identityErrorDescriptions));
            EnsureIdentityOperationSucceeded(
                await _userManager.AddLoginAsync(provisionedUser, new UserLoginInfo(
                    ExternalLoginProviderNames.EntraId,
                    validatedExternalIdentity.SubjectId,
                    ExternalLoginProviderNames.EntraIdDisplayName)),
                identityErrorDescriptions => string.Format(ExceptionMessages.UserCreationFailedTemplate, provisionedUsername, identityErrorDescriptions));

            await _auditLogService.LogActionAsync(AuditAction.UserProvisioned, provisionedUser.Id);
            return provisionedUser;
        }

        private async Task SynchronizeUserRolesAsync(IdentityUser user, IReadOnlyList<string> mappedRoleNames)
        {
            var currentRoleNames = await _userManager.GetRolesAsync(user);
            var removedRoleNames = currentRoleNames.Except(mappedRoleNames).ToList();
            var addedRoleNames = mappedRoleNames.Except(currentRoleNames).ToList();

            if (removedRoleNames.Count > 0)
            {
                EnsureIdentityOperationSucceeded(
                    await _userManager.RemoveFromRolesAsync(user, removedRoleNames),
                    identityErrorDescriptions => string.Format(ExceptionMessages.RoleAssignmentFailedTemplate, string.Join(", ", removedRoleNames), user.UserName, identityErrorDescriptions));
            }

            if (addedRoleNames.Count > 0)
            {
                EnsureIdentityOperationSucceeded(
                    await _userManager.AddToRolesAsync(user, addedRoleNames),
                    identityErrorDescriptions => string.Format(ExceptionMessages.RoleAssignmentFailedTemplate, string.Join(", ", addedRoleNames), user.UserName, identityErrorDescriptions));
            }
        }
    }
}
