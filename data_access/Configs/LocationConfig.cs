using business_logic.Constants;
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
            builder.Property(x => x.City).IsRequired().HasMaxLength(LocationConstraints.CityMaxLength);
            builder.Property(x => x.Country).IsRequired().HasMaxLength(LocationConstraints.CountryMaxLength);
            builder.Property(x => x.TimeZoneId).IsRequired().HasMaxLength(LocationConstraints.TimeZoneIdMaxLength);
            builder.HasIndex(x => x.GeoNamesId)
                .IsUnique()
                .HasFilter("[GeoNamesId] IS NOT NULL")
                .HasDatabaseName(UniqueIndexNames.LocationGeoNamesId);
        }
    }
}
