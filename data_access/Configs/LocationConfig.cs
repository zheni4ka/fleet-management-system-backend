using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    internal class LocationConfig : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("Locations");
            builder.ConfigureArchiving();
            builder.Property(x => x.City).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Country).IsRequired().HasMaxLength(200);
        }
    }
}
