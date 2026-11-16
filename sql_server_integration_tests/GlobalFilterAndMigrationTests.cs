using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using data_access.data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using sql_server_integration_tests.Infrastructure;

namespace sql_server_integration_tests
{
    [Collection(SqlServerTestCollection.Name)]
    public sealed class GlobalFilterAndMigrationTests
    {
        private readonly SqlServerDatabaseFixture _sqlServerDatabaseFixture;

        public GlobalFilterAndMigrationTests(SqlServerDatabaseFixture sqlServerDatabaseFixture)
        {
            _sqlServerDatabaseFixture = sqlServerDatabaseFixture;
        }

        [SkippableFact]
        public async Task MigrateAsync_AllMigrationsAppliedOnEmptyDatabase_LeavesNoPendingMigrations()
        {
            Skip.If(_sqlServerDatabaseFixture.SkipReason is not null, _sqlServerDatabaseFixture.SkipReason);

            await _sqlServerDatabaseFixture.RunInNewScopeAsync(async scopedServiceProvider =>
            {
                var dbContext = scopedServiceProvider.GetRequiredService<FMS_DbContext>();
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
                var assemblyMigrations = dbContext.Database.GetMigrations();

                Assert.Empty(pendingMigrations);
                Assert.Equal(assemblyMigrations.Order(), appliedMigrations.Order());
            });
        }

        [SkippableFact]
        public async Task GetRoutesPagedListBySearchFilterAsync_DispatcherScopedToOneDepot_ReturnsOnlyRoutesOfThatDepot()
        {
            Skip.If(_sqlServerDatabaseFixture.SkipReason is not null, _sqlServerDatabaseFixture.SkipReason);

            var otherDepot = new Depot { Name = $"Депо фільтра {Guid.NewGuid():N}" };
            await _sqlServerDatabaseFixture.SeedAsync(otherDepot);
            var ownDepotAuto = BusinessLogicTestHost.CreateAuto("ФЛ0001ТР");
            var ownDepotDriver = BusinessLogicTestHost.CreateDriver("ФЛТ 000001");
            var otherDepotAuto = BusinessLogicTestHost.CreateAuto("ФЛ0002ТР", depotId: otherDepot.Id);
            var otherDepotDriver = BusinessLogicTestHost.CreateDriver("ФЛТ 000002", depotId: otherDepot.Id);
            var departure = DateTime.Today.AddDays(200);
            var ownDepotRoute = BusinessLogicTestHost.CreateRoute(ownDepotAuto, ownDepotDriver, departure, departure.AddHours(2));
            var otherDepotRoute = BusinessLogicTestHost.CreateRoute(otherDepotAuto, otherDepotDriver, departure, departure.AddHours(2));
            await _sqlServerDatabaseFixture.SeedAsync(ownDepotRoute, otherDepotRoute);

            _sqlServerDatabaseFixture.CurrentUserScope.IsDepotScopeUnrestricted = false;
            _sqlServerDatabaseFixture.CurrentUserScope.AccessibleDepotIds = [ownDepotAuto.DepotId];
            try
            {
                var routesPage = await _sqlServerDatabaseFixture.RunInNewScopeAsync<IRouteService, GetPagedListResponseModel<GetRouteResponseModel>>(
                    routeService => routeService.GetRoutesPagedListBySearchFilterAsync(
                        new GetRoutesBySearchFilterRequestModel(null, null, DateOnly.FromDateTime(departure), DateOnly.FromDateTime(departure), null, null, null, null),
                        new GetPagedListRequestModel<RouteSortField>(Page: 1, PageSize: 100, Sort: null, SortDirection: null, Search: null)));

                Assert.Contains(routesPage.Items, route => route.Id == ownDepotRoute.Id);
                Assert.DoesNotContain(routesPage.Items, route => route.Id == otherDepotRoute.Id);
                Assert.All(routesPage.Items, route => Assert.Equal(ownDepotAuto.DepotId, route.DepotId));
            }
            finally
            {
                _sqlServerDatabaseFixture.CurrentUserScope.IsDepotScopeUnrestricted = true;
                _sqlServerDatabaseFixture.CurrentUserScope.AccessibleDepotIds = [];
            }
        }
    }
}
