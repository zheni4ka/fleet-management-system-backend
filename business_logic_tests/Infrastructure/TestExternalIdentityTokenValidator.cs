using business_logic.DTOs;
using business_logic.Interfaces;

namespace business_logic_tests.Infrastructure
{
    public sealed class TestExternalIdentityTokenValidator : IExternalIdentityTokenValidator
    {
        public ValidatedExternalIdentityModel? ValidatedExternalIdentity { get; set; }

        public Task<ValidatedExternalIdentityModel> ValidateIdTokenAsync(string idToken, string expectedNonce)
        {
            if (ValidatedExternalIdentity is null)
            {
                throw new InvalidOperationException("The test did not set the validated external identity.");
            }

            return Task.FromResult(ValidatedExternalIdentity);
        }
    }
}
