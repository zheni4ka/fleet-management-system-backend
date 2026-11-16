using business_logic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace data_access.Configs
{
    public class UserDepotConfig : IEntityTypeConfiguration<UserDepot>
    {
        public void Configure(EntityTypeBuilder<UserDepot> builder)
        {
            builder.HasKey(userDepot => new { userDepot.UserId, userDepot.DepotId });
            builder.ToTable("UserDepots");

            builder.HasOne(userDepot => userDepot.User)
                .WithMany()
                .HasForeignKey(userDepot => userDepot.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(userDepot => userDepot.Depot)
                .WithMany()
                .HasForeignKey(userDepot => userDepot.DepotId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
