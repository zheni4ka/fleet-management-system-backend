using business_logic.Constants;
using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    internal class DriverConfig : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RowVersion).IsRowVersion();
            builder.ConfigureDepotOwnership();
            builder.ConfigureArchiving();
            builder.ToTable("Drivers");

            builder.Property(d => d.Name)
                .HasMaxLength(DriverConstraints.NameMaxLength);

            builder.Property(d => d.Surname)
                .HasMaxLength(DriverConstraints.SurnameMaxLength);

            builder.Property(d => d.Patronymic)
                .HasMaxLength(DriverConstraints.PatronymicMaxLength);

            builder.HasIndex(d => d.LicenseNumber)
                .IsUnique()
                .HasDatabaseName(UniqueIndexNames.DriverLicenseNumber);
        }
    }
}
