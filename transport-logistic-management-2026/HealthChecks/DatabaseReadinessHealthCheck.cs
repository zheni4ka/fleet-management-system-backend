using data_access.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace transport_logistic_management_2026.HealthChecks
{
    public class DatabaseReadinessHealthCheck : IHealthCheck
    {
        private readonly FMS_DbContext _context;

        public DatabaseReadinessHealthCheck(FMS_DbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!await _context.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Unhealthy("The database is not reachable.");
            }

            var pendingMigrations = (await _context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            if (pendingMigrations.Count > 0)
            {
                return HealthCheckResult.Unhealthy($"The database has {pendingMigrations.Count} pending migrations: {string.Join(", ", pendingMigrations)}.");
            }

            return HealthCheckResult.Healthy();
        }
    }
}
