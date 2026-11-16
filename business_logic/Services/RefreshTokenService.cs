using Ardalis.Specification;
using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Helpers;
using business_logic.Interfaces;
using business_logic.Settings;
using business_logic.Specifications;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace business_logic.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RefreshTokenSettings _refreshTokenSettings;

        public RefreshTokenService(
            IRepository<RefreshToken> refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IOptions<RefreshTokenSettings> refreshTokenSettingsOptions)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _refreshTokenSettings = refreshTokenSettingsOptions.Value;
        }

        public async Task<IssuedRefreshTokenModel> IssueRefreshTokenInNewFamilyAsync(string userId)
        {
            var issuedRefreshToken = await AddRefreshTokenAsync(userId, Guid.NewGuid());
            await _unitOfWork.SaveChangesAsync();
            return issuedRefreshToken;
        }

        public async Task<RotatedRefreshTokenModel> RotateRefreshTokenAsync(string refreshToken)
        {
            var storedRefreshToken = await GetStoredRefreshTokenAsync(refreshToken);

            var currentUtcTime = DateTime.UtcNow;
            if (storedRefreshToken.ExpiresAtUtc <= currentUtcTime)
            {
                throw new UnauthorizedAccessException(ExceptionMessages.RefreshTokenInvalid);
            }

            if (storedRefreshToken.RevokedAtUtc is { } revokedAtUtc)
            {
                var isRotatedTokenReusedWithinGracePeriod = storedRefreshToken.RevocationReason == RefreshTokenRevocationReason.Rotated
                    && currentUtcTime - revokedAtUtc < TimeSpan.FromSeconds(_refreshTokenSettings.RotatedTokenReuseGracePeriodInSeconds);
                if (!isRotatedTokenReusedWithinGracePeriod)
                {
                    await RevokeUnrevokedRefreshTokensAsync(
                        new RefreshTokenSpecs.UnrevokedByFamilyId(storedRefreshToken.FamilyId),
                        RefreshTokenRevocationReason.ReuseDetected);
                    await _unitOfWork.SaveChangesAsync();
                    throw new UnauthorizedAccessException(ExceptionMessages.RefreshTokenInvalid);
                }
            }
            else
            {
                storedRefreshToken.RevokedAtUtc = currentUtcTime;
                storedRefreshToken.RevocationReason = RefreshTokenRevocationReason.Rotated;
            }

            var issuedRefreshToken = await AddRefreshTokenAsync(storedRefreshToken.UserId, storedRefreshToken.FamilyId);
            await _unitOfWork.SaveChangesAsync();
            return new RotatedRefreshTokenModel(UserId: storedRefreshToken.UserId, IssuedRefreshToken: issuedRefreshToken);
        }

        public async Task RevokeRefreshTokenFamilyAsync(string refreshToken, RefreshTokenRevocationReason revocationReason)
        {
            var storedRefreshToken = await GetStoredRefreshTokenAsync(refreshToken);
            await RevokeUnrevokedRefreshTokensAsync(new RefreshTokenSpecs.UnrevokedByFamilyId(storedRefreshToken.FamilyId), revocationReason);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RevokeAllUserRefreshTokensAsync(string userId, RefreshTokenRevocationReason revocationReason)
        {
            await RevokeUnrevokedRefreshTokensAsync(new RefreshTokenSpecs.UnrevokedByUserId(userId), revocationReason);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<RefreshToken> GetStoredRefreshTokenAsync(string refreshToken)
        {
            return await _refreshTokenRepository.GetItemBySpecAsync(
                new RefreshTokenSpecs.ByTokenHash(RefreshTokenHasher.GetRefreshTokenHash(refreshToken)))
                ?? throw new UnauthorizedAccessException(ExceptionMessages.RefreshTokenInvalid);
        }

        private async Task RevokeUnrevokedRefreshTokensAsync(
            ISpecification<RefreshToken> unrevokedRefreshTokensSpecification,
            RefreshTokenRevocationReason revocationReason)
        {
            var revocationUtcTime = DateTime.UtcNow;
            foreach (var unrevokedRefreshToken in await _refreshTokenRepository.GetListBySpecAsync(unrevokedRefreshTokensSpecification))
            {
                unrevokedRefreshToken.RevokedAtUtc = revocationUtcTime;
                unrevokedRefreshToken.RevocationReason = revocationReason;
            }
        }

        private async Task<IssuedRefreshTokenModel> AddRefreshTokenAsync(string userId, Guid familyId)
        {
            var refreshToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(AccountConstraints.RefreshTokenByteLength));
            var createdAtUtc = DateTime.UtcNow;
            var expiresAtUtc = createdAtUtc.AddDays(_refreshTokenSettings.ExpiryInDays);

            await _refreshTokenRepository.InsertAsync(new RefreshToken
            {
                UserId = userId,
                TokenHash = RefreshTokenHasher.GetRefreshTokenHash(refreshToken),
                FamilyId = familyId,
                CreatedAtUtc = createdAtUtc,
                ExpiresAtUtc = expiresAtUtc
            });

            return new IssuedRefreshTokenModel(RefreshToken: refreshToken, ExpiresAtUtc: new DateTimeOffset(expiresAtUtc));
        }
    }
}
