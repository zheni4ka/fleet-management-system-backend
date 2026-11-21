using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using business_logic.Validators;
using business_logic_tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Reflection;

namespace business_logic_tests
{
    public sealed class LocalizationTests : IDisposable
    {
        private readonly BusinessLogicTestHost _testHost = new();

        public static TheoryData<Type, string> LocalizedMessageProperties()
        {
            var localizedMessageProperties = new TheoryData<Type, string>();
            foreach (var localizedMessagesType in new[] { typeof(ExceptionMessages), typeof(ValidationMessages), typeof(EntityDisplayNames), typeof(FieldDisplayNames) })
            {
                foreach (var localizedMessageProperty in localizedMessagesType.GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    localizedMessageProperties.Add(localizedMessagesType, localizedMessageProperty.Name);
                }
            }

            return localizedMessageProperties;
        }

        [Theory]
        [MemberData(nameof(LocalizedMessageProperties))]
        public void LocalizedMessage_EveryKey_HasDistinctUkrainianAndEnglishText(Type localizedMessagesType, string localizedMessageKey)
        {
            var ukrainianText = ReadLocalizedMessage(localizedMessagesType, localizedMessageKey, SupportedLanguages.Ukrainian);
            var englishText = ReadLocalizedMessage(localizedMessagesType, localizedMessageKey, SupportedLanguages.English);

            Assert.False(string.IsNullOrWhiteSpace(ukrainianText));
            Assert.False(string.IsNullOrWhiteSpace(englishText));
            Assert.NotEqual(ukrainianText, englishText);
        }

        [Theory]
        [InlineData(SupportedLanguages.English, "The field \"Start location\" must be greater than 0.", "The start and destination locations must differ.")]
        [InlineData(SupportedLanguages.Ukrainian, "Поле «Початкова локація» має бути більшим за 0.", "Початкова та кінцева локації мають відрізнятися.")]
        public void CreateRouteRequestModelValidator_InvalidLocations_ReturnsMessagesInRequestLanguage(
            string cultureName, string expectedStartLocationMessage, string expectedSameLocationsMessage)
        {
            var routeRequestModel = new CreateRouteRequestModel
            {
                StartLocationId = 0,
                DestinationLocationId = 0,
                AutoId = 1,
                DriverId = 1,
                DepartureTimeUtc = new DateTime(2030, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                ArrivalTimeUtc = new DateTime(2030, 1, 1, 18, 0, 0, DateTimeKind.Utc)
            };

            var validationErrorMessages = ValidateInCulture(routeRequestModel, cultureName);

            Assert.Contains(expectedStartLocationMessage, validationErrorMessages);
            Assert.Contains(expectedSameLocationsMessage, validationErrorMessages);
        }

        [Fact]
        public async Task UpdatePreferredLanguageAsync_EnglishChosen_IsReturnedWithNextSession()
        {
            var user = new IdentityUser { UserName = "dispatcher_language", Email = "dispatcher_language@example.com", EmailConfirmed = true };
            await _testHost.RunInNewScopeAsync<UserManager<IdentityUser>, IdentityResult>(userManager => userManager.CreateAsync(user, "Valid1Password"));
            var loginRequestModel = new LoginRequestModel { Username = user.UserName, Password = "Valid1Password" };
            var firstLoginResponse = await _testHost.RunInNewScopeAsync<IAccountService, LoginResponseModel>(accountService => accountService.LoginAsync(loginRequestModel));

            _testHost.CurrentUserScope.UserId = user.Id;
            await _testHost.RunInNewScopeAsync<IAccountService>(accountService => accountService.UpdatePreferredLanguageAsync(
                new UpdatePreferredLanguageRequestModel { Language = SupportedLanguages.English }));
            var secondLoginResponse = await _testHost.RunInNewScopeAsync<IAccountService, LoginResponseModel>(accountService => accountService.LoginAsync(loginRequestModel));

            Assert.Equal(SupportedLanguages.Default, firstLoginResponse.PreferredLanguage);
            Assert.Equal(SupportedLanguages.English, secondLoginResponse.PreferredLanguage);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }

        private static IReadOnlyList<string> ValidateInCulture(CreateRouteRequestModel routeRequestModel, string cultureName)
        {
            var previousUiCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
            try
            {
                return new CreateRouteRequestModelValidator().Validate(routeRequestModel).Errors
                    .Select(validationFailure => validationFailure.ErrorMessage)
                    .ToList();
            }
            finally
            {
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }

        private static string ReadLocalizedMessage(Type localizedMessagesType, string localizedMessageKey, string cultureName)
        {
            var previousUiCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
            try
            {
                return (string)localizedMessagesType.GetProperty(localizedMessageKey, BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
            }
            finally
            {
                CultureInfo.CurrentUICulture = previousUiCulture;
            }
        }
    }
}
