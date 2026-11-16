using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IExternalLoginService
    {
        Task<LoginWithExternalIdentityResponseModel> LoginWithExternalIdentityAsync(LoginWithExternalIdentityRequestModel model);
    }
}
