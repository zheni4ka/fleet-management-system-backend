using business_logic.DTOs;
using business_logic.Entities;

namespace business_logic.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<IssuedRefreshTokenModel> IssueRefreshTokenInNewFamilyAsync(string userId);
        Task<RotatedRefreshTokenModel> RotateRefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenFamilyAsync(string refreshToken, RefreshTokenRevocationReason revocationReason);
        Task RevokeAllUserRefreshTokensAsync(string userId, RefreshTokenRevocationReason revocationReason);
    }
}
