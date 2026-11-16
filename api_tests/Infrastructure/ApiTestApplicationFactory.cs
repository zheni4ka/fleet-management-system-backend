using data_access.data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using transport_logistic_management_2026;
using transport_logistic_management_2026.Services;

namespace api_tests.Infrastructure
{
    public sealed class ApiTestApplicationFactory : WebApplicationFactory<Program>
    {
        public const string TestEnvironmentName = "Testing";

        private readonly SqliteConnection _sqliteConnection;
        private readonly IReadOnlyDictionary<string, string> _configurationOverrides;

        public ApiTestApplicationFactory(IReadOnlyDictionary<string, string> configurationOverrides)
        {
            _configurationOverrides = configurationOverrides;
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _sqliteConnection.Open();

            using var schemaDbContext = new FMS_DbContext(CreateSqliteDbContextOptions());
            schemaDbContext.Database.EnsureCreated();
        }

        public static IReadOnlyDictionary<string, string> CreateValidConfiguration()
        {
            return new Dictionary<string, string>
            {
                ["ConnectionStrings:connectionString"] = "Data Source=replaced-by-sqlite-in-tests",
                ["JwtSettings:Secret"] = "integration-test-signing-secret-that-is-long-enough-for-hs256",
                ["JwtSettings:Issuer"] = "http://localhost",
                ["JwtSettings:Audience"] = "http://localhost:3000",
                ["JwtSettings:AccessTokenExpiryInMinutes"] = "15",
                ["RefreshTokenSettings:ExpiryInDays"] = "7",
                ["RefreshTokenSettings:RotatedTokenReuseGracePeriodInSeconds"] = "30",
                ["RateLimitingSettings:CredentialEndpointsPermitLimitPerIpAddress"] = "3",
                ["RateLimitingSettings:CredentialEndpointsWindowInSeconds"] = "60",
                ["RateLimitingSettings:GeneralPermitLimitPerUser"] = "300",
                ["RateLimitingSettings:GeneralWindowInSeconds"] = "60",
                ["CorsSettings:AllowedOrigins:0"] = "http://localhost:3000",
                ["SeedAdmin:Password"] = "Integration1Admin"
            };
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(TestEnvironmentName);
            foreach (var (configurationKey, configurationValue) in _configurationOverrides)
            {
                builder.UseSetting(configurationKey, configurationValue);
            }

            builder.ConfigureTestServices(services =>
            {
                var replacedServiceDescriptors = services
                    .Where(serviceDescriptor => serviceDescriptor.ServiceType == typeof(DbContextOptions<FMS_DbContext>)
                        || serviceDescriptor.ServiceType == typeof(IDbContextOptionsConfiguration<FMS_DbContext>)
                        || serviceDescriptor.ImplementationType == typeof(DueMaintenanceAutoStatusSyncBackgroundService))
                    .ToList();
                foreach (var replacedServiceDescriptor in replacedServiceDescriptors)
                {
                    services.Remove(replacedServiceDescriptor);
                }

                services.AddDbContext<FMS_DbContext>(dbContextOptions => dbContextOptions.UseSqlite(_sqliteConnection));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _sqliteConnection.Dispose();
            }
        }

        private DbContextOptions<FMS_DbContext> CreateSqliteDbContextOptions()
        {
            return new DbContextOptionsBuilder<FMS_DbContext>().UseSqlite(_sqliteConnection).Options;
        }
    }
}
