using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace business_logic_tests
{
    public sealed class AccountSessionTests : IDisposable
    {
        private const string TestUsername = "dispatcher_session";
        private const string TestPassword = "Valid1Password";
        private const string WrongPassword = "Wrong1Password";
        private const string ChangedPassword = "Changed2Password";

        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task RefreshSessionAsync_ValidRefreshToken_IssuesDifferentRefreshToken()
        {
            await CreateTestUserAsync(_testHost);
            var loginResponse = await LoginAsync(_testHost, TestPassword);

            var refreshSessionResponse = await RefreshSessionAsync(_testHost, loginResponse.RefreshToken);

            Assert.NotEqual(loginResponse.RefreshToken, refreshSessionResponse.RefreshToken);
            Assert.Equal(TestUsername, refreshSessionResponse.Username);
        }

        [Fact]
        public async Task RefreshSessionAsync_RotatedTokenReused_RevokesWholeTokenFamily()
        {
            await CreateTestUserAsync(_testHost);
            var loginResponse = await LoginAsync(_testHost, TestPassword);
            var refreshSessionResponse = await RefreshSessionAsync(_testHost, loginResponse.RefreshToken);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => RefreshSessionAsync(_testHost, loginResponse.RefreshToken));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => RefreshSessionAsync(_testHost, refreshSessionResponse.RefreshToken));
        }

        [Fact]
        public async Task RefreshSessionAsync_RotatedTokenReusedWithinGracePeriod_KeepsTokenFamilyActive()
        {
            using var gracePeriodTestHost = new BusinessLogicTestHost(rotatedTokenReuseGracePeriodInSeconds: 60);
            await CreateTestUserAsync(gracePeriodTestHost);
            var loginResponse = await LoginAsync(gracePeriodTestHost, TestPassword);
            var firstRefreshSessionResponse = await RefreshSessionAsync(gracePeriodTestHost, loginResponse.RefreshToken);

            var concurrentRefreshSessionResponse = await RefreshSessionAsync(gracePeriodTestHost, loginResponse.RefreshToken);
            var followingRefreshSessionResponse = await RefreshSessionAsync(gracePeriodTestHost, firstRefreshSessionResponse.RefreshToken);

            Assert.NotEqual(firstRefreshSessionResponse.RefreshToken, concurrentRefreshSessionResponse.RefreshToken);
            Assert.Equal(TestUsername, followingRefreshSessionResponse.Username);
        }

        [Fact]
        public async Task LogoutAsync_RevokesRefreshTokenFamily()
        {
            await CreateTestUserAsync(_testHost);
            var loginResponse = await LoginAsync(_testHost, TestPassword);

            await _testHost.RunInNewScopeAsync<IAccountService>(accountService => accountService.LogoutAsync(
                new LogoutRequestModel { RefreshToken = loginResponse.RefreshToken }));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => RefreshSessionAsync(_testHost, loginResponse.RefreshToken));
        }

        [Fact]
        public async Task LoginAsync_MaxFailedAttemptsReached_LocksAccountOutEvenForCorrectPassword()
        {
            await CreateTestUserAsync(_testHost);
            for (var failedAttemptNumber = 1; failedAttemptNumber < AccountConstraints.LockoutMaxFailedAccessAttempts; failedAttemptNumber++)
            {
                var wrongPasswordException = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => LoginAsync(_testHost, WrongPassword));
                Assert.Equal(ExceptionMessages.WrongUsernameOrPassword, wrongPasswordException.Message);
            }

            var lockingAttemptException = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => LoginAsync(_testHost, WrongPassword));
            var correctPasswordException = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => LoginAsync(_testHost, TestPassword));

            Assert.Equal(ExceptionMessages.AccountLockedOut, lockingAttemptException.Message);
            Assert.Equal(ExceptionMessages.AccountLockedOut, correctPasswordException.Message);
        }

        [Fact]
        public async Task ResetUserPasswordAsync_RevokesSessionsAndRequiresPasswordChangeBeforeLogin()
        {
            var userId = await CreateTestUserAsync(_testHost);
            var loginResponse = await LoginAsync(_testHost, TestPassword);

            var resetUserPasswordResponse = await _testHost.RunInNewScopeAsync<IAccountService, ResetUserPasswordResponseModel>(
                accountService => accountService.ResetUserPasswordAsync(new ResetUserPasswordRequestModel { UserId = userId }));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => RefreshSessionAsync(_testHost, loginResponse.RefreshToken));
            await Assert.ThrowsAsync<PasswordChangeRequiredException>(() => LoginAsync(_testHost, resetUserPasswordResponse.TemporaryPassword));

            var changePasswordResponse = await _testHost.RunInNewScopeAsync<IAccountService, ChangePasswordResponseModel>(
                accountService => accountService.ChangePasswordAsync(new ChangePasswordRequestModel
                {
                    Username = TestUsername,
                    CurrentPassword = resetUserPasswordResponse.TemporaryPassword,
                    NewPassword = ChangedPassword
                }));
            var loginWithChangedPasswordResponse = await LoginAsync(_testHost, ChangedPassword);

            Assert.Equal(TestUsername, changePasswordResponse.Username);
            Assert.Equal(TestUsername, loginWithChangedPasswordResponse.Username);
        }

        [Fact]
        public async Task DeactivateUserAsync_RevokesSessionsAndRejectsLoginWithDeactivatedMessage()
        {
            var userId = await CreateTestUserAsync(_testHost);
            var loginResponse = await LoginAsync(_testHost, TestPassword);

            await _testHost.RunInNewScopeAsync<IAccountService>(accountService => accountService.DeactivateUserAsync(userId));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => RefreshSessionAsync(_testHost, loginResponse.RefreshToken));
            var deactivatedLoginException = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => LoginAsync(_testHost, TestPassword));
            Assert.Equal(ExceptionMessages.AccountDeactivated, deactivatedLoginException.Message);

            await _testHost.RunInNewScopeAsync<IAccountService>(accountService => accountService.ReactivateUserAsync(userId));
            var reactivatedLoginResponse = await LoginAsync(_testHost, TestPassword);
            Assert.Equal(TestUsername, reactivatedLoginResponse.Username);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }

        private static async Task<string> CreateTestUserAsync(BusinessLogicTestHost testHost)
        {
            var testUser = new IdentityUser { UserName = TestUsername, Email = "dispatcher_session@example.com", EmailConfirmed = true };
            var createUserResult = await testHost.RunInNewScopeAsync<UserManager<IdentityUser>, IdentityResult>(
                userManager => userManager.CreateAsync(testUser, TestPassword));
            Assert.True(createUserResult.Succeeded, string.Join(" ", createUserResult.Errors.Select(identityError => identityError.Description)));
            return testUser.Id;
        }

        private static Task<LoginResponseModel> LoginAsync(BusinessLogicTestHost testHost, string password)
        {
            return testHost.RunInNewScopeAsync<IAccountService, LoginResponseModel>(accountService => accountService.LoginAsync(
                new LoginRequestModel { Username = TestUsername, Password = password }));
        }

        private static Task<RefreshSessionResponseModel> RefreshSessionAsync(BusinessLogicTestHost testHost, string refreshToken)
        {
            return testHost.RunInNewScopeAsync<IAccountService, RefreshSessionResponseModel>(accountService => accountService.RefreshSessionAsync(
                new RefreshSessionRequestModel { RefreshToken = refreshToken }));
        }
    }
}
