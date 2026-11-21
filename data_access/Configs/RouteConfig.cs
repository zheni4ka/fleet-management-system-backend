using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace data_access.Configs
{
    public class RouteConfig : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RowVersion).IsRowVersion();
            builder.ConfigureDepotOwnership();
            builder.Property(r => r.DepartureTimeUtc).HasConversion(
                utcTime => utcTime,
                persistedUtcTime => DateTime.SpecifyKind(persistedUtcTime, DateTimeKind.Utc));
            builder.Property(r => r.ArrivalTimeUtc).HasConversion(
                utcTime => utcTime,
                persistedUtcTime => DateTime.SpecifyKind(persistedUtcTime, DateTimeKind.Utc));
            builder.Property(r => r.ActualDepartureTimeUtc).HasConversion(
                utcTime => utcTime,
                persistedUtcTime => persistedUtcTime.HasValue ? DateTime.SpecifyKind(persistedUtcTime.Value, DateTimeKind.Utc) : persistedUtcTime);
            builder.Property(r => r.ActualArrivalTimeUtc).HasConversion(
                utcTime => utcTime,
                persistedUtcTime => persistedUtcTime.HasValue ? DateTime.SpecifyKind(persistedUtcTime.Value, DateTimeKind.Utc) : persistedUtcTime);
            builder.ToTable("Routes");

            builder.HasIndex(r => r.DepartureTimeUtc);

            builder.HasIndex(r => new { r.AutoId, r.Status, r.DepartureTimeUtc });

            builder.HasIndex(r => new { r.DriverId, r.Status, r.DepartureTimeUtc });

            builder.HasOne(r => r.Auto)
                   .WithMany(a => a.Routes)
                   .HasForeignKey(r => r.AutoId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Driver)
                .WithMany(a => a.Routes)
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(r => r.Status).HasDefaultValue(RouteStatus.Planned);

            builder.HasOne(r => r.StartLocation)
                .WithMany(l => l.StartRoutes)
                .HasForeignKey(r => r.StartLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.DestinationLocation)
                .WithMany(l => l.DestinationRoutes)
                .HasForeignKey(r => r.DestinationLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
