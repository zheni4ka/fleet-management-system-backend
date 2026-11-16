using business_logic.Constants;
using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(refreshToken => refreshToken.Id);
            builder.ToTable("RefreshTokens");

            builder.Property(refreshToken => refreshToken.TokenHash)
                .HasMaxLength(AccountConstraints.RefreshTokenHashLength)
                .IsFixedLength()
                .IsUnicode(false)
                .IsRequired();

            builder.HasIndex(refreshToken => refreshToken.TokenHash)
                .IsUnique();

            builder.HasIndex(refreshToken => refreshToken.FamilyId);
            builder.HasIndex(refreshToken => refreshToken.UserId);

            builder.Property(refreshToken => refreshToken.CreatedAtUtc)
                .HasConversion(
                    utcTime => utcTime,
                    persistedUtcTime => DateTime.SpecifyKind(persistedUtcTime, DateTimeKind.Utc));

            builder.Property(refreshToken => refreshToken.ExpiresAtUtc)
                .HasConversion(
                    utcTime => utcTime,
                    persistedUtcTime => DateTime.SpecifyKind(persistedUtcTime, DateTimeKind.Utc));

            builder.Property(refreshToken => refreshToken.RevokedAtUtc)
                .HasConversion(
                    utcTime => utcTime,
                    persistedUtcTime => persistedUtcTime.HasValue ? DateTime.SpecifyKind(persistedUtcTime.Value, DateTimeKind.Utc) : null);

            builder.Property(refreshToken => refreshToken.RevocationReason)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.HasOne(refreshToken => refreshToken.User)
                .WithMany()
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
