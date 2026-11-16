using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IExternalIdentityTokenValidator
    {
        Task<ValidatedExternalIdentityModel> ValidateIdTokenAsync(string idToken, string expectedNonce);
    }
}
