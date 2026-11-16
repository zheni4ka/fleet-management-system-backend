using Microsoft.AspNetCore.Identity;

namespace business_logic.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public Guid FamilyId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public RefreshTokenRevocationReason? RevocationReason { get; set; }
    }
}
