using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    public class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("AuditLogs");

            builder.HasIndex(x => x.Time);

            builder.Property(x => x.Time)
                .HasConversion(
                    utcTime => utcTime,
                    persistedUtcTime => DateTime.SpecifyKind(persistedUtcTime, DateTimeKind.Utc));

            builder.Property(x => x.Action)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.EntityId)
                .HasMaxLength(450);

            builder.HasOne(x => x.Dispatcher)
                .WithMany()
                .HasForeignKey(x => x.DispatcherId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
