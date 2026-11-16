using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace business_logic_tests
{
    public sealed class ExternalLoginTests : IDisposable
    {
        private const string ExternalSubjectId = "test-tenant:object-1";
        private const string ExternalUsername = "dispatcher@contoso.example";

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task LoginWithExternalIdentityAsync_FirstLogin_ProvisionsLinkedUserWithMappedRole()
        {
            await SeedRolesAsync();
            await SetValidatedExternalIdentityAsync(BusinessLogicTestHost.DispatcherGroupId);

            var loginResponse = await LoginWithExternalIdentityAsync();

            var linkedUser = await _testHost.RunInNewScopeAsync<UserManager<IdentityUser>, IdentityUser?>(
                userManager => userManager.FindByLoginAsync(ExternalLoginProviderNames.EntraId, ExternalSubjectId));
            Assert.NotNull(linkedUser);
            Assert.Equal(ExternalUsername, loginResponse.Username);
            Assert.Equal([UserRoles.Dispatcher], loginResponse.Roles);
        }

        [Fact]
        public async Task LoginWithExternalIdentityAsync_GroupsChangedInIdentityProvider_SynchronizesRolesOfLinkedUser()
        {
            await SeedRolesAsync();
            await SetValidatedExternalIdentityAsync(BusinessLogicTestHost.DispatcherGroupId);
            await LoginWithExternalIdentityAsync();

            await SetValidatedExternalIdentityAsync(BusinessLogicTestHost.AdminGroupId);
            var secondLoginResponse = await LoginWithExternalIdentityAsync();

            Assert.Equal([UserRoles.Admin], secondLoginResponse.Roles);
        }

        [Fact]
        public async Task LoginWithExternalIdentityAsync_NoMappedGroup_ThrowsAccessDeniedWithoutProvisioningUser()
        {
            await SeedRolesAsync();
            await SetValidatedExternalIdentityAsync("group-without-access");

            await Assert.ThrowsAsync<ExternalIdentityAccessDeniedException>(LoginWithExternalIdentityAsync);

            var linkedUser = await _testHost.RunInNewScopeAsync<UserManager<IdentityUser>, IdentityUser?>(
                userManager => userManager.FindByLoginAsync(ExternalLoginProviderNames.EntraId, ExternalSubjectId));
            Assert.Null(linkedUser);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }

        private async Task SeedRolesAsync()
        {
            foreach (var roleName in UserRoles.All)
            {
                await _testHost.RunInNewScopeAsync<RoleManager<IdentityRole>, IdentityResult>(
                    roleManager => roleManager.CreateAsync(new IdentityRole(roleName)));
            }
        }

        private Task SetValidatedExternalIdentityAsync(string groupId)
        {
            return _testHost.RunInNewScopeAsync<TestExternalIdentityTokenValidator>(testExternalIdentityTokenValidator =>
            {
                testExternalIdentityTokenValidator.ValidatedExternalIdentity = new ValidatedExternalIdentityModel(
                    SubjectId: ExternalSubjectId,
                    PreferredUsername: ExternalUsername,
                    Email: ExternalUsername,
                    GroupIds: [groupId]);
                return Task.CompletedTask;
            });
        }

        private Task<LoginWithExternalIdentityResponseModel> LoginWithExternalIdentityAsync()
        {
            return _testHost.RunInNewScopeAsync<IExternalLoginService, LoginWithExternalIdentityResponseModel>(
                externalLoginService => externalLoginService.LoginWithExternalIdentityAsync(new LoginWithExternalIdentityRequestModel
                {
                    IdToken = "test-id-token",
                    Nonce = "test-nonce"
                }));
        }
    }
}
