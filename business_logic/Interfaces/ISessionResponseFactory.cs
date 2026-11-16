using business_logic.DTOs;
using Microsoft.AspNetCore.Identity;

namespace business_logic.Interfaces
{
    public interface ISessionResponseFactory
    {
        Task<TSessionResponseModel> CreateSessionResponseModelAsync<TSessionResponseModel>(
            IdentityUser user,
            IssuedRefreshTokenModel issuedRefreshToken)
            where TSessionResponseModel : SessionResponseModelBase, new();
    }
}
