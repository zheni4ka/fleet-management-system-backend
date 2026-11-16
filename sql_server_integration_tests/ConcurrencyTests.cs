using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using sql_server_integration_tests.Infrastructure;

namespace sql_server_integration_tests
{
    [Collection(SqlServerTestCollection.Name)]
    public sealed class ConcurrencyTests
    {
        private const int ParallelCreationRoundsCount = 10;

        private readonly SqlServerDatabaseFixture _sqlServerDatabaseFixture;

        public ConcurrencyTests(SqlServerDatabaseFixture sqlServerDatabaseFixture)
        {
            _sqlServerDatabaseFixture = sqlServerDatabaseFixture;
        }

        [SkippableFact]
        public async Task CreateAsync_TwoOverlappingRoutesForSameAutoAndDriverInParallel_ExactlyOneSucceeds()
        {
            Skip.If(_sqlServerDatabaseFixture.SkipReason is not null, _sqlServerDatabaseFixture.SkipReason);

            for (var parallelCreationRound = 0; parallelCreationRound < ParallelCreationRoundsCount; parallelCreationRound++)
            {
                var auto = BusinessLogicTestHost.CreateAuto($"ПР{parallelCreationRound:D4}АА");
                var driver = BusinessLogicTestHost.CreateDriver($"ПРЛ {parallelCreationRound:D6}");
                await _sqlServerDatabaseFixture.SeedAsync(auto, driver);
                var departure = DateTime.Today.AddDays(30 + parallelCreationRound).AddHours(8);

                var parallelCreationOutcomes = await Task.WhenAll(
                    TryCreateRouteAsync(auto.Id, driver.Id, departure, departure.AddHours(4)),
                    TryCreateRouteAsync(auto.Id, driver.Id, departure.AddHours(1), departure.AddHours(5)));

                Assert.Single(parallelCreationOutcomes, isRouteCreated => isRouteCreated);
            }
        }

        [SkippableFact]
        public async Task SaveChangesAsync_DuplicateAutoNumberBypassingPreCheck_ThrowsUniqueConstraintViolationWithNumber()
        {
            Skip.If(_sqlServerDatabaseFixture.SkipReason is not null, _sqlServerDatabaseFixture.SkipReason);

            const string duplicatedAutoNumber = "ДУ0001БЛ";
            await _sqlServerDatabaseFixture.SeedAsync(BusinessLogicTestHost.CreateAuto(duplicatedAutoNumber));

            var uniqueConstraintViolationException = await Assert.ThrowsAsync<UniqueConstraintViolationException>(() =>
                _sqlServerDatabaseFixture.RunInNewScopeAsync(async scopedServiceProvider =>
                {
                    await scopedServiceProvider.GetRequiredService<IRepository<Auto>>().InsertAsync(BusinessLogicTestHost.CreateAuto(duplicatedAutoNumber));
                    await scopedServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
                }));

            Assert.Equal(string.Format(ExceptionMessages.AutoNumberAlreadyExistsTemplate, duplicatedAutoNumber), uniqueConstraintViolationException.Message);
        }

        [SkippableFact]
        public async Task TryAcquireSessionExclusiveLockWithoutWaitingAsync_LockHeldByAnotherSession_ReturnsNull()
        {
            Skip.If(_sqlServerDatabaseFixture.SkipReason is not null, _sqlServerDatabaseFixture.SkipReason);

            await _sqlServerDatabaseFixture.RunInNewScopeAsync<IDatabaseApplicationLockService>(async firstInstanceLockService =>
            {
                await using var firstInstanceJobLock = await firstInstanceLockService.TryAcquireSessionExclusiveLockWithoutWaitingAsync(
                    ApplicationLockResourceNames.DueMaintenanceAutoStatusSyncJob);
                Assert.NotNull(firstInstanceJobLock);

                await _sqlServerDatabaseFixture.RunInNewScopeAsync<IDatabaseApplicationLockService>(async secondInstanceLockService =>
                {
                    var secondInstanceJobLock = await secondInstanceLockService.TryAcquireSessionExclusiveLockWithoutWaitingAsync(
                        ApplicationLockResourceNames.DueMaintenanceAutoStatusSyncJob);
                    Assert.Null(secondInstanceJobLock);
                });
            });
        }

        private async Task<bool> TryCreateRouteAsync(int autoId, int driverId, DateTime departure, DateTime arrival)
        {
            try
            {
                await _sqlServerDatabaseFixture.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService =>
                    routeService.CreateAsync(RouteRequestModelFactory.CreateRouteRequestModelFor(autoId, driverId, departure, arrival)));
                return true;
            }
            catch (AssignmentConflictException)
            {
                return false;
            }
        }
    }
}
