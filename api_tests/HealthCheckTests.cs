using api_tests.Infrastructure;
using System.Net;

namespace api_tests
{
    public sealed class HealthCheckTests
    {
        [Fact]
        public async Task Liveness_ProcessRunning_ReturnsHealthyWithoutCheckingDatabase()
        {
            using var apiTestApplicationFactory = new ApiTestApplicationFactory(ApiTestApplicationFactory.CreateValidConfiguration());
            using var apiHttpClient = apiTestApplicationFactory.CreateClient();

            var livenessResponse = await apiHttpClient.GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, livenessResponse.StatusCode);
        }

        [Fact]
        public async Task Readiness_DatabaseWithPendingMigrations_ReturnsServiceUnavailable()
        {
            using var apiTestApplicationFactory = new ApiTestApplicationFactory(ApiTestApplicationFactory.CreateValidConfiguration());
            using var apiHttpClient = apiTestApplicationFactory.CreateClient();

            var readinessResponse = await apiHttpClient.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, readinessResponse.StatusCode);
        }
    }
}
