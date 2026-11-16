using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class DepotScopingAndArchivingTests : IDisposable
    {
        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task GetPagedListAsync_DispatcherScopedToOneDepot_ReturnsOnlyAutosOfThatDepot()
        {
            var otherDepot = new Depot { Name = "Львівське депо" };
            await _testHost.SeedAsync(otherDepot);
            var ownDepotAuto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var otherDepotAuto = BusinessLogicTestHost.CreateAuto("ВС2222ВС", depotId: otherDepot.Id);
            await _testHost.SeedAsync(ownDepotAuto, otherDepotAuto);
            ScopeCurrentUserToDepots(ownDepotAuto.DepotId);

            var autosPage = await _testHost.RunInNewScopeAsync<IAutoService, GetPagedListResponseModel<GetAutoResponseModel>>(
                autoService => autoService.GetPagedListAsync(
                    new GetAutosBySearchFilterRequestModel(Status: null, Color: null, IsArchived: false, DepotId: null),
                    new GetPagedListRequestModel<AutoSortField>(Page: 1, PageSize: 20, Sort: null, SortDirection: null, Search: null)));

            Assert.Equal([ownDepotAuto.Id], autosPage.Items.Select(auto => auto.Id));
            Assert.Equal(1, autosPage.TotalCount);
        }

        [Fact]
        public async Task CreateAsync_DispatcherAssignsAutoOfForeignDepot_ThrowsNotFound()
        {
            var otherDepot = new Depot { Name = "Львівське депо" };
            await _testHost.SeedAsync(otherDepot);
            var otherDepotAuto = BusinessLogicTestHost.CreateAuto("ВС2222ВС", depotId: otherDepot.Id);
            var ownDepotDriver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(otherDepotAuto, ownDepotDriver);
            ScopeCurrentUserToDepots(ownDepotDriver.DepotId);

            await Assert.ThrowsAsync<NotFoundException>(() => CreateRouteAsync(otherDepotAuto.Id, ownDepotDriver.Id));
        }

        [Fact]
        public async Task CreateAsync_DriverFromAnotherDepotThanAuto_ThrowsBusinessRuleWithBothDepots()
        {
            var otherDepot = new Depot { Name = "Львівське депо" };
            await _testHost.SeedAsync(otherDepot);
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var otherDepotDriver = BusinessLogicTestHost.CreateDriver("ABC 111111", depotId: otherDepot.Id);
            await _testHost.SeedAsync(auto, otherDepotDriver);

            var depotMismatchException = await Assert.ThrowsAsync<BusinessRuleException>(() => CreateRouteAsync(auto.Id, otherDepotDriver.Id));

            Assert.Contains($"#{otherDepot.Id}", depotMismatchException.Message);
            Assert.Contains($"#{auto.DepotId}", depotMismatchException.Message);
        }

        [Fact]
        public async Task DeleteAsync_AutoReferencedByCompletedRoute_ArchivesAndHidesFromLookupsUntilRestored()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var completedRoute = BusinessLogicTestHost.CreateRoute(auto, driver, DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1), RouteStatus.Completed);
            await _testHost.SeedAsync(completedRoute);

            var deleteAutoResponse = await _testHost.RunInNewScopeAsync<IAutoService, DeleteAutoResponseModel>(autoService => autoService.DeleteAsync(auto.Id));
            var lookupAutoIdsAfterArchiving = await GetLookupAutoIdsAsync();
            var historicalRoute = await _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.GetAsync(completedRoute.Id));
            await _testHost.RunInNewScopeAsync<IAutoService>(autoService => autoService.RestoreAsync(auto.Id));
            var lookupAutoIdsAfterRestoring = await GetLookupAutoIdsAsync();

            Assert.True(deleteAutoResponse.IsArchivedInsteadOfDeleted);
            Assert.DoesNotContain(auto.Id, lookupAutoIdsAfterArchiving);
            Assert.Equal(auto.Number, historicalRoute.AutoNumber);
            Assert.Contains(auto.Id, lookupAutoIdsAfterRestoring);
        }

        [Fact]
        public async Task DeleteAsync_AutoWithoutRoutes_DeletesPermanently()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            await _testHost.SeedAsync(auto);

            var deleteAutoResponse = await _testHost.RunInNewScopeAsync<IAutoService, DeleteAutoResponseModel>(autoService => autoService.DeleteAsync(auto.Id));

            Assert.False(deleteAutoResponse.IsArchivedInsteadOfDeleted);
            await Assert.ThrowsAsync<NotFoundException>(() => _testHost.RunInNewScopeAsync<IAutoService, GetAutoResponseModel>(autoService => autoService.GetAsync(auto.Id)));
        }

        [Fact]
        public async Task DeleteAsync_AutoWithPlannedRoute_ThrowsBusinessRule()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(BusinessLogicTestHost.CreateRoute(auto, driver, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2)));

            await Assert.ThrowsAsync<BusinessRuleException>(() => _testHost.RunInNewScopeAsync<IAutoService, DeleteAutoResponseModel>(autoService => autoService.DeleteAsync(auto.Id)));
        }

        [Fact]
        public async Task CreateAsync_ArchivedDriver_ThrowsBusinessRule()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var archivedDriver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            archivedDriver.IsArchived = true;
            archivedDriver.ArchivedAtUtc = DateTime.UtcNow;
            await _testHost.SeedAsync(auto, archivedDriver);

            await Assert.ThrowsAsync<BusinessRuleException>(() => CreateRouteAsync(auto.Id, archivedDriver.Id));
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }

        private void ScopeCurrentUserToDepots(params int[] accessibleDepotIds)
        {
            _testHost.CurrentUserScope.IsDepotScopeUnrestricted = false;
            _testHost.CurrentUserScope.AccessibleDepotIds = accessibleDepotIds;
        }

        private Task<GetRouteResponseModel> CreateRouteAsync(int autoId, int driverId)
        {
            var departure = DateTime.Today.AddDays(3).AddHours(8);
            return _testHost.RunInNewScopeAsync<IRouteService, GetRouteResponseModel>(routeService => routeService.CreateAsync(
                RouteRequestModelFactory.CreateRouteRequestModelFor(autoId, driverId, departure, departure.AddHours(4))));
        }

        private async Task<IReadOnlyList<int>> GetLookupAutoIdsAsync()
        {
            var autoLookupOptions = await _testHost.RunInNewScopeAsync<IAutoService, IReadOnlyList<GetAutoLookupOptionResponseModel>>(
                autoService => autoService.GetLookupOptionsAsync());
            return autoLookupOptions.Select(autoLookupOption => autoLookupOption.Id).ToList();
        }
    }
}
