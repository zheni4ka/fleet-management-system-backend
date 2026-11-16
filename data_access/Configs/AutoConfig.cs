using business_logic.Constants;
using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    internal class AutoConfig : IEntityTypeConfiguration<Auto>
    {
        public void Configure(EntityTypeBuilder<Auto> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("Autos");

            builder.HasIndex(a => a.Number)
                .IsUnique();

            builder.Property(a => a.Mark)
                .HasMaxLength(AutoConstraints.MarkMaxLength);

            builder.Property(a => a.Model)
                .HasMaxLength(AutoConstraints.ModelMaxLength);

            builder.Property(a => a.Color)
                .HasMaxLength(AutoConstraints.ColorMaxLength);

            builder.Property(a => a.Capacity)
                .IsRequired();

            builder.Property(a => a.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);
        }
    }
}
