using business_logic.Constants;
using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    public class AutoMaintenanceConfig : IEntityTypeConfiguration<AutoMaintenance>
    {
        public void Configure(EntityTypeBuilder<AutoMaintenance> builder)
        {
            builder.ToTable("AutoMaintenance");
            builder.HasKey(x => x.Id);

            builder.Property(am => am.Name)
                .HasMaxLength(AutoMaintenanceConstraints.NameMaxLength);

            builder.Property(am => am.Description)
                .HasMaxLength(AutoMaintenanceConstraints.DescriptionMaxLength);

            builder.HasIndex(am => new { am.AutoId, am.IsCompleted, am.ServiceDate });

            builder.HasOne(x => x.Auto)
                   .WithMany(a => a.Services)
                   .HasForeignKey(x => x.AutoId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
