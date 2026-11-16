using System.Reflection;
using business_logic.Constants;
using business_logic.Entities;
using business_logic.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace data_access.data
{
    public class FMS_DbContext : IdentityDbContext
    {
        private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
        private const string SqliteRandomRowVersionSql = "randomblob(8)";

        private readonly ICurrentUserService _currentUserService;

        public FMS_DbContext(DbContextOptions<FMS_DbContext> opt, ICurrentUserService currentUserService) : base(opt)
        {
            _currentUserService = currentUserService;
        }

        private bool IsDepotScopeUnrestricted => _currentUserService.IsDepotScopeUnrestricted;

        private IReadOnlyCollection<int> AccessibleDepotIds => _currentUserService.AccessibleDepotIds;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.SeedData();

            ApplyDepotScopeFilter<Auto>(modelBuilder);
            ApplyDepotScopeFilter<Driver>(modelBuilder);
            ApplyDepotScopeFilter<Route>(modelBuilder);
            modelBuilder.Entity<Depot>().HasQueryFilter(
                QueryFilterNames.DepotScope,
                depot => IsDepotScopeUnrestricted || AccessibleDepotIds.Contains(depot.Id));

            if (Database.ProviderName == SqliteProviderName)
            {
                ConfigureSqliteRowVersionDefaults(modelBuilder);
            }
        }

        private void ApplyDepotScopeFilter<TDepotScopedEntity>(ModelBuilder modelBuilder)
            where TDepotScopedEntity : class, IBelongsToDepot
        {
            modelBuilder.Entity<TDepotScopedEntity>().HasQueryFilter(
                QueryFilterNames.DepotScope,
                depotScopedEntity => IsDepotScopeUnrestricted || AccessibleDepotIds.Contains(depotScopedEntity.DepotId));
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
