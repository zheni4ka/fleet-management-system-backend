using System.Reflection;
using business_logic.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace data_access.data
{
    public class FMS_DbContext : IdentityDbContext
    {
        private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
        private const string SqliteRandomRowVersionSql = "randomblob(8)";

        public FMS_DbContext(DbContextOptions<FMS_DbContext> opt) : base(opt)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.SeedData();

            if (Database.ProviderName == SqliteProviderName)
            {
                ConfigureSqliteRowVersionDefaults(modelBuilder);
            }
        }

        private static void ConfigureSqliteRowVersionDefaults(ModelBuilder modelBuilder)
        {
            var rowVersionedEntityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(entityType => typeof(IHasRowVersion).IsAssignableFrom(entityType.ClrType));
            foreach (var rowVersionedEntityType in rowVersionedEntityTypes)
            {
                modelBuilder.Entity(rowVersionedEntityType.ClrType)
                    .Property(nameof(IHasRowVersion.RowVersion))
                    .HasDefaultValueSql(SqliteRandomRowVersionSql);
            }
        }
    }
}
