using business_logic.Constants;
using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    public class DepotConfig : IEntityTypeConfiguration<Depot>
    {
        public void Configure(EntityTypeBuilder<Depot> builder)
        {
            builder.HasKey(depot => depot.Id);
            builder.ToTable("Depots");

            builder.Property(depot => depot.Name)
                .HasMaxLength(DepotConstraints.NameMaxLength)
                .IsRequired();

            builder.HasIndex(depot => depot.Name)
                .IsUnique()
                .HasDatabaseName(UniqueIndexNames.DepotName);

            builder.HasData(new Depot { Id = DepotConstraints.DefaultDepotId, Name = DepotConstraints.DefaultDepotName });
        }
    }
}
