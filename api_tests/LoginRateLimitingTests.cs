using api_tests.Infrastructure;
using business_logic.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;

namespace api_tests
{
    public sealed class LoginRateLimitingTests
    {
        private const string LoginApiPath = "/api/v1/Account/login";
        private const string UnversionedLoginApiPath = "/api/Account/login";
        private const int TestCredentialEndpointsPermitLimit = 3;

        [Fact]
        public async Task Login_PermitLimitExceededFromSameIpAddress_ReturnsTooManyRequestsWithUkrainianMessage()
        {
            using var apiTestApplicationFactory = new ApiTestApplicationFactory(ApiTestApplicationFactory.CreateValidConfiguration());
            using var apiHttpClient = apiTestApplicationFactory.CreateClient();

            for (var permittedLoginAttemptNumber = 1; permittedLoginAttemptNumber <= TestCredentialEndpointsPermitLimit; permittedLoginAttemptNumber++)
            {
                var permittedLoginResponse = await PostUnknownUserLoginAsync(apiHttpClient);
                Assert.Equal(HttpStatusCode.Unauthorized, permittedLoginResponse.StatusCode);
            }

            var rejectedLoginResponse = await PostUnknownUserLoginAsync(apiHttpClient);
            var rejectedLoginProblem = await rejectedLoginResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.Equal(HttpStatusCode.TooManyRequests, rejectedLoginResponse.StatusCode);
            Assert.Equal(ExceptionMessages.TooManyRequests, rejectedLoginProblem!.Detail);
        }

        [Fact]
        public void Startup_AccessTokenExpiryNotPositive_FailsWithConfigurationKeyAndValueInMessage()
        {
            var configurationWithZeroAccessTokenExpiry = new Dictionary<string, string>(ApiTestApplicationFactory.CreateValidConfiguration())
            {
                ["JwtSettings:AccessTokenExpiryInMinutes"] = "0"
            };
            using var apiTestApplicationFactory = new ApiTestApplicationFactory(configurationWithZeroAccessTokenExpiry);

            var startupException = Assert.Throws<OptionsValidationException>(() => apiTestApplicationFactory.CreateClient());

            Assert.Contains("JwtSettings:AccessTokenExpiryInMinutes", startupException.Message);
            Assert.Contains("'0'", startupException.Message);
        }

        [Fact]
        public async Task Login_UnversionedRoute_ReturnsNotFound()
        {
            using var apiTestApplicationFactory = new ApiTestApplicationFactory(ApiTestApplicationFactory.CreateValidConfiguration());
            using var apiHttpClient = apiTestApplicationFactory.CreateClient();

            var unversionedLoginResponse = await apiHttpClient.PostAsJsonAsync(UnversionedLoginApiPath, new { username = "unknown_user", password = "Unknown1Password" });

            Assert.Equal(HttpStatusCode.NotFound, unversionedLoginResponse.StatusCode);
        }

        private static Task<HttpResponseMessage> PostUnknownUserLoginAsync(HttpClient apiHttpClient)
        {
            return apiHttpClient.PostAsJsonAsync(LoginApiPath, new { username = "unknown_user", password = "Unknown1Password" });
        }
    }
}
