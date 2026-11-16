using business_logic;
using business_logic.Interfaces;
using business_logic.Settings;
using business_logic_tests.Infrastructure;
using data_access;
using data_access.data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace sql_server_integration_tests.Infrastructure
{
    public sealed class SqlServerDatabaseFixture : IAsyncLifetime
    {
        public const string ConnectionStringEnvironmentVariableName = "SQL_SERVER_INTEGRATION_TESTS_CONNECTION_STRING";
        private const string SqlServerContainerImage = "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04";
        private const string TestDatabaseNamePrefix = "FleetIntegrationTests_";

        private MsSqlContainer? _sqlServerContainer;
        private ServiceProvider? _serviceProvider;
        private string? _testDatabaseConnectionString;

        public string? SkipReason { get; private set; }

        public async Task InitializeAsync()
        {
            var serverConnectionString = await GetServerConnectionStringAsync();
            if (serverConnectionString is null)
            {
                return;
            }

            _testDatabaseConnectionString = new SqlConnectionStringBuilder(serverConnectionString)
            {
                InitialCatalog = TestDatabaseNamePrefix + Guid.NewGuid().ToString("N"),
                MultipleActiveResultSets = true
            }.ConnectionString;

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddBusinessLogicServices();
            services.AddDataAccessServices(_testDatabaseConnectionString);
            services.AddAuthentication();
            services.AddSingleton<TestCurrentUserScope>();
            services.AddScoped<ICurrentUserService, TestCurrentUserService>();
            services.AddScoped<ITokenService, TestTokenService>();
            services.AddSingleton<IExternalIdentityTokenValidator, TestExternalIdentityTokenValidator>();
            services.AddOptions<AuditLogRetentionSettings>().Configure(auditLogRetentionSettings =>
            {
                auditLogRetentionSettings.RetentionPeriodInDays = BusinessLogicTestHost.TestAuditLogRetentionPeriodInDays;
            });
            services.AddOptions<RefreshTokenSettings>().Configure(refreshTokenSettings =>
            {
                refreshTokenSettings.ExpiryInDays = BusinessLogicTestHost.TestRefreshTokenExpiryInDays;
            });
            _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            using var migrationScope = _serviceProvider.CreateScope();
            await migrationScope.ServiceProvider.GetRequiredService<FMS_DbContext>().Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            if (_serviceProvider is not null)
            {
                using (var cleanupScope = _serviceProvider.CreateScope())
                {
                    await cleanupScope.ServiceProvider.GetRequiredService<FMS_DbContext>().Database.EnsureDeletedAsync();
                }

                await _serviceProvider.DisposeAsync();
            }

            if (_sqlServerContainer is not null)
            {
                await _sqlServerContainer.DisposeAsync();
            }
        }

        public TestCurrentUserScope CurrentUserScope => GetServiceProvider().GetRequiredService<TestCurrentUserScope>();

        public async Task RunInNewScopeAsync(Func<IServiceProvider, Task> scopedServicesCall)
        {
            using var serviceScope = GetServiceProvider().CreateScope();
            await scopedServicesCall(serviceScope.ServiceProvider);
        }

        public async Task RunInNewScopeAsync<TService>(Func<TService, Task> serviceCall) where TService : notnull
        {
            using var serviceScope = GetServiceProvider().CreateScope();
            await serviceCall(serviceScope.ServiceProvider.GetRequiredService<TService>());
        }

        public async Task<TResult> RunInNewScopeAsync<TService, TResult>(Func<TService, Task<TResult>> serviceCall) where TService : notnull
        {
            using var serviceScope = GetServiceProvider().CreateScope();
            return await serviceCall(serviceScope.ServiceProvider.GetRequiredService<TService>());
        }

        public async Task SeedAsync(params object[] seededEntities)
        {
            using var serviceScope = GetServiceProvider().CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<FMS_DbContext>();
            dbContext.AddRange(seededEntities);
            await dbContext.SaveChangesAsync();
        }

        private ServiceProvider GetServiceProvider()
        {
            return _serviceProvider ?? throw new InvalidOperationException(SkipReason ?? "SQL Server fixture was not initialized.");
        }

        private async Task<string?> GetServerConnectionStringAsync()
        {
            var configuredConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariableName);
            if (!string.IsNullOrWhiteSpace(configuredConnectionString))
            {
                return configuredConnectionString;
            }

            try
            {
                _sqlServerContainer = new MsSqlBuilder(SqlServerContainerImage).Build();
                await _sqlServerContainer.StartAsync();
                return _sqlServerContainer.GetConnectionString();
            }
            catch (Exception dockerStartException)
            {
                SkipReason = $"SQL Server integration tests skipped: Docker is unavailable ({dockerStartException.GetType().Name}: {dockerStartException.Message}). "
                    + $"Start Docker or set {ConnectionStringEnvironmentVariableName} to a SQL Server connection string.";
                _sqlServerContainer = null;
                return null;
            }
        }
    }
}
