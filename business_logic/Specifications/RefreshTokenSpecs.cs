using Ardalis.Specification;
using business_logic.Entities;

namespace business_logic.Specifications
{
    public class RefreshTokenSpecs
    {
        public class ByTokenHash : Specification<RefreshToken>
        {
            public ByTokenHash(string tokenHash)
            {
                Query.Where(refreshToken => refreshToken.TokenHash == tokenHash);
            }
        }

        public class UnrevokedByFamilyId : Specification<RefreshToken>
        {
            public UnrevokedByFamilyId(Guid familyId)
            {
                Query.Where(refreshToken => refreshToken.FamilyId == familyId && refreshToken.RevokedAtUtc == null);
            }
        }

        public class UnrevokedByUserId : Specification<RefreshToken>
        {
            public UnrevokedByUserId(string userId)
            {
                Query.Where(refreshToken => refreshToken.UserId == userId && refreshToken.RevokedAtUtc == null);
            }
        }
    }
}
