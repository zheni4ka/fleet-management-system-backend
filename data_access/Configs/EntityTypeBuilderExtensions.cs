using business_logic.Entities;
using business_logic.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    public static class EntityTypeBuilderExtensions
    {
        public static void ConfigureArchiving<TArchivableEntity>(this EntityTypeBuilder<TArchivableEntity> builder)
            where TArchivableEntity : class, IArchivable
        {
            builder.Property(archivableEntity => archivableEntity.ArchivedAtUtc)
                .HasConversion(
                    utcTime => utcTime,
                    persistedUtcTime => persistedUtcTime.HasValue ? DateTime.SpecifyKind(persistedUtcTime.Value, DateTimeKind.Utc) : null);

            builder.HasIndex(archivableEntity => archivableEntity.IsArchived);
        }

        public static void ConfigureDepotOwnership<TDepotScopedEntity>(this EntityTypeBuilder<TDepotScopedEntity> builder)
            where TDepotScopedEntity : class, IBelongsToDepot
        {
            builder.HasOne(typeof(Depot), nameof(IBelongsToDepot.Depot))
                .WithMany()
                .HasForeignKey(nameof(IBelongsToDepot.DepotId))
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(depotScopedEntity => depotScopedEntity.DepotId);
        }
    }
}
