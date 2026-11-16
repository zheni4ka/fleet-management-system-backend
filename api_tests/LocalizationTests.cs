using api_tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace api_tests
{
    public sealed class LocalizationTests
    {
        private const string LoginApiPath = "/api/v1/Account/login";

        [Theory]
        [InlineData("en", "Wrong username or password.")]
        [InlineData("en-US,en;q=0.9", "Wrong username or password.")]
        [InlineData("uk", "Неправильне ім'я користувача або пароль.")]
        [InlineData("de", "Неправильне ім'я користувача або пароль.")]
        public async Task Login_WrongCredentials_ReturnsMessageInRequestedLanguageWithUkrainianFallback(string acceptLanguage, string expectedProblemDetail)
        {
            using var apiTestApplicationFactory = new ApiTestApplicationFactory(ApiTestApplicationFactory.CreateValidConfiguration());
            using var apiHttpClient = apiTestApplicationFactory.CreateClient();
            using var loginRequest = new HttpRequestMessage(HttpMethod.Post, LoginApiPath)
            {
                Content = JsonContent.Create(new { username = "unknown_user", password = "Unknown1Password" })
            };
            loginRequest.Headers.TryAddWithoutValidation("Accept-Language", acceptLanguage);

            var loginResponse = await apiHttpClient.SendAsync(loginRequest);
            var loginProblem = await loginResponse.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
            Assert.Equal(expectedProblemDetail, loginProblem!.Detail);
        }
    }
}
