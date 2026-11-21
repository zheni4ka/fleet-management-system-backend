using business_logic.Constants;
using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    public class DriverTripConfig : IEntityTypeConfiguration<DriverTrip>
    {
        public void Configure(EntityTypeBuilder<DriverTrip> builder)
        {
            builder.ToTable("DriverTrips");
            builder.HasKey(x => x.Id);
            builder.ConfigureDepotOwnership();

            builder.HasOne(driverTrip => driverTrip.Driver)
                .WithMany(driver => driver.Trips)
                .HasForeignKey(driverTrip => driverTrip.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(driverTrip => driverTrip.Route)
                .WithMany()
                .HasForeignKey(driverTrip => driverTrip.RouteId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(driverTrip => driverTrip.RouteId)
                .IsUnique()
                .HasFilter("[RouteId] IS NOT NULL");
            builder.HasIndex(driverTrip => new { driverTrip.DriverId, driverTrip.StartedAtUtc });

            builder.Property(driverTrip => driverTrip.AutoMark).HasMaxLength(AutoConstraints.MarkMaxLength);
            builder.Property(driverTrip => driverTrip.AutoModel).HasMaxLength(AutoConstraints.ModelMaxLength);
            builder.Property(driverTrip => driverTrip.AutoNumber).HasMaxLength(DriverTripConstraints.AutoNumberMaxLength);
            builder.Property(driverTrip => driverTrip.StartLocationCity).HasMaxLength(LocationConstraints.CityMaxLength);
            builder.Property(driverTrip => driverTrip.StartLocationCountry).HasMaxLength(LocationConstraints.CountryMaxLength);
            builder.Property(driverTrip => driverTrip.StartLocationTimeZoneId).HasMaxLength(LocationConstraints.TimeZoneIdMaxLength);
            builder.Property(driverTrip => driverTrip.DestinationLocationCity).HasMaxLength(LocationConstraints.CityMaxLength);
            builder.Property(driverTrip => driverTrip.DestinationLocationCountry).HasMaxLength(LocationConstraints.CountryMaxLength);
            builder.Property(driverTrip => driverTrip.DestinationLocationTimeZoneId).HasMaxLength(LocationConstraints.TimeZoneIdMaxLength);
            builder.Property(driverTrip => driverTrip.StartAddressLine).HasMaxLength(AddressGeocodingConstraints.AddressLineMaxLength);
            builder.Property(driverTrip => driverTrip.DestinationAddressLine).HasMaxLength(AddressGeocodingConstraints.AddressLineMaxLength);

            builder.Property(driverTrip => driverTrip.PlannedDepartureTimeUtc).HasConversion(
                utcTime => utcTime,
                persistedUtcTime => DateTime.SpecifyKind(persistedUtcTime, DateTimeKind.Utc));
            builder.Property(driverTrip => driverTrip.PlannedArrivalTimeUtc).HasConversion(
                utcTime => utcTime,
                persistedUtcTime => DateTime.SpecifyKind(persistedUtcTime, DateTimeKind.Utc));
            builder.Property(driverTrip => driverTrip.StartedAtUtc).HasConversion(
                utcTime => utcTime,
                persistedUtcTime => DateTime.SpecifyKind(persistedUtcTime, DateTimeKind.Utc));
            builder.Property(driverTrip => driverTrip.EndedAtUtc).HasConversion(
                utcTime => utcTime,
                persistedUtcTime => DateTime.SpecifyKind(persistedUtcTime, DateTimeKind.Utc));
        }
    }
}
