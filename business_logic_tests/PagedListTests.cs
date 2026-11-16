using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace business_logic_tests
{
    public sealed class PagedListTests : IDisposable
    {
        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task AutoGetPagedListAsync_SecondPage_ReturnsPageItemsAndTotalCount()
        {
            var seededAutos = Enumerable.Range(1, 25)
                .Select(autoIndex => BusinessLogicTestHost.CreateAuto($"АА{autoIndex:0000}АА"))
                .ToArray<object>();
            await _testHost.SeedAsync(seededAutos);

            var autosPage = await _testHost.RunInNewScopeAsync<IAutoService, GetPagedListResponseModel<GetAutoResponseModel>>(
                autoService => autoService.GetPagedListAsync(
                    new GetAutosBySearchFilterRequestModel(Status: null, Color: null),
                    new GetPagedListRequestModel<AutoSortField>(
                        Page: 2, PageSize: 10, Sort: AutoSortField.Number, SortDirection: SortDirection.Descending, Search: null)));

            Assert.Equal(25, autosPage.TotalCount);
            Assert.Equal(10, autosPage.Items.Count);
            Assert.Equal("АА0015АА", autosPage.Items[0].Number);
            Assert.Equal("АА0006АА", autosPage.Items[^1].Number);
        }

        [Fact]
        public async Task AutoGetPagedListAsync_Search_FiltersBeforeCounting()
        {
            await _testHost.SeedAsync(
                BusinessLogicTestHost.CreateAuto("АА0001АА", mark: "Volvo"),
                BusinessLogicTestHost.CreateAuto("АА0002АА", mark: "Scania"),
                BusinessLogicTestHost.CreateAuto("АА0003АА", mark: "Scania"));

            var autosPage = await _testHost.RunInNewScopeAsync<IAutoService, GetPagedListResponseModel<GetAutoResponseModel>>(
                autoService => autoService.GetPagedListAsync(
                    new GetAutosBySearchFilterRequestModel(Status: null, Color: null),
                    new GetPagedListRequestModel<AutoSortField>(
                        Page: 1, PageSize: 1, Sort: null, SortDirection: null, Search: " Scania ")));

            Assert.Equal(2, autosPage.TotalCount);
            Assert.Single(autosPage.Items);
            Assert.Equal("Scania", autosPage.Items[0].Mark);
        }

        [Fact]
        public async Task AutoGetPagedListAsync_StatusFilter_ReturnsOnlyAutosWithStatus()
        {
            await _testHost.SeedAsync(
                BusinessLogicTestHost.CreateAuto("АА0001АА", AutoStatus.Available),
                BusinessLogicTestHost.CreateAuto("АА0002АА", AutoStatus.UnderMaintenance),
                BusinessLogicTestHost.CreateAuto("АА0003АА", AutoStatus.UnderMaintenance));

            var autosPage = await _testHost.RunInNewScopeAsync<IAutoService, GetPagedListResponseModel<GetAutoResponseModel>>(
                autoService => autoService.GetPagedListAsync(
                    new GetAutosBySearchFilterRequestModel(Status: AutoStatus.UnderMaintenance, Color: null),
                    new GetPagedListRequestModel<AutoSortField>(
                        Page: 1, PageSize: 10, Sort: null, SortDirection: null, Search: null)));

            Assert.Equal(2, autosPage.TotalCount);
            Assert.All(autosPage.Items, auto => Assert.Equal(AutoStatus.UnderMaintenance, auto.Status));
        }

        [Fact]
        public async Task DriverGetPagedListAsync_NumericSearch_MatchesExactId()
        {
            var seededDrivers = Enumerable.Range(1, 12)
                .Select(driverIndex => BusinessLogicTestHost.CreateDriver($"LICENSE {(char)('A' + driverIndex)}"))
                .ToArray<object>();
            await _testHost.SeedAsync(seededDrivers);

            var driversPage = await _testHost.RunInNewScopeAsync<IDriverService, GetPagedListResponseModel<GetDriverResponseModel>>(
                driverService => driverService.GetPagedListAsync(new GetPagedListRequestModel<DriverSortField>(
                    Page: 1, PageSize: 10, Sort: null, SortDirection: null, Search: "12")));

            Assert.Equal(1, driversPage.TotalCount);
            Assert.Equal(12, driversPage.Items[0].Id);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, PagedListConstraints.MaxPageSize + 1)]
        public async Task DriverGetPagedListAsync_InvalidPaging_ThrowsValidationException(int page, int pageSize)
        {
            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<IDriverService, GetPagedListResponseModel<GetDriverResponseModel>>(
                driverService => driverService.GetPagedListAsync(new GetPagedListRequestModel<DriverSortField>(
                    Page: page, PageSize: pageSize, Sort: null, SortDirection: null, Search: null))));
        }

        [Fact]
        public async Task GetDispatchersPagedListAsync_UsersInOtherRoles_ReturnsOnlyDispatchers()
        {
            await _testHost.RunInNewScopeAsync<RoleManager<IdentityRole>>(async roleManager =>
            {
                foreach (var roleName in UserRoles.All)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            });
            await _testHost.RunInNewScopeAsync<UserManager<IdentityUser>>(async userManager =>
            {
                await CreateUserInRoleAsync(userManager, "dispatcher_b", UserRoles.Dispatcher);
                await CreateUserInRoleAsync(userManager, "dispatcher_a", UserRoles.Dispatcher);
                await CreateUserInRoleAsync(userManager, "admin_user", UserRoles.Admin);
            });

            var dispatchersPage = await _testHost.RunInNewScopeAsync<IAccountService, GetPagedListResponseModel<GetDispatcherResponseModel>>(
                accountService => accountService.GetDispatchersPagedListAsync(new GetPagedListRequestModel<DispatcherSortField>(
                    Page: 1, PageSize: 10, Sort: DispatcherSortField.Username, SortDirection: SortDirection.Ascending, Search: null)));

            Assert.Equal(2, dispatchersPage.TotalCount);
            Assert.Equal(new[] { "dispatcher_a", "dispatcher_b" }, dispatchersPage.Items.Select(dispatcher => dispatcher.Username));
        }

        [Fact]
        public async Task AuditLogGetPagedListAsync_ActionFilter_ReturnsNewestMatchingLogsFirst()
        {
            await _testHost.SeedAsync(
                new AuditLog { Action = AuditAction.AutoCreated, EntityId = "1", Time = new DateTime(2030, 1, 1, 10, 0, 0) },
                new AuditLog { Action = AuditAction.AutoCreated, EntityId = "2", Time = new DateTime(2030, 1, 2, 10, 0, 0) },
                new AuditLog { Action = AuditAction.RouteCreated, EntityId = "3", Time = new DateTime(2030, 1, 3, 10, 0, 0) });

            var auditLogsPage = await _testHost.RunInNewScopeAsync<IAuditLogService, GetPagedListResponseModel<GetAuditLogResponseModel>>(
                auditLogService => auditLogService.GetPagedListAsync(
                    new GetAuditLogsBySearchFilterRequestModel(Action: AuditAction.AutoCreated, TimeFromDate: null, TimeToDate: null),
                    new GetPagedListRequestModel<AuditLogSortField>(
                        Page: 1, PageSize: 10, Sort: null, SortDirection: null, Search: null)));

            Assert.Equal(2, auditLogsPage.TotalCount);
            Assert.Equal(new[] { "2", "1" }, auditLogsPage.Items.Select(auditLog => auditLog.EntityId));
        }

        [Fact]
        public async Task AuditLogGetPagedListAsync_DateRangeFilter_IncludesWholeKyivEndDay()
        {
            await _testHost.SeedAsync(
                new AuditLog { Action = AuditAction.AutoCreated, EntityId = "1", Time = new DateTime(2030, 1, 1, 21, 59, 0, DateTimeKind.Utc) },
                new AuditLog { Action = AuditAction.AutoCreated, EntityId = "2", Time = new DateTime(2030, 1, 1, 22, 0, 0, DateTimeKind.Utc) },
                new AuditLog { Action = AuditAction.AutoCreated, EntityId = "3", Time = new DateTime(2030, 1, 3, 21, 59, 0, DateTimeKind.Utc) },
                new AuditLog { Action = AuditAction.AutoCreated, EntityId = "4", Time = new DateTime(2030, 1, 3, 22, 0, 0, DateTimeKind.Utc) });

            var auditLogsPage = await _testHost.RunInNewScopeAsync<IAuditLogService, GetPagedListResponseModel<GetAuditLogResponseModel>>(
                auditLogService => auditLogService.GetPagedListAsync(
                    new GetAuditLogsBySearchFilterRequestModel(Action: null, TimeFromDate: new DateOnly(2030, 1, 2), TimeToDate: new DateOnly(2030, 1, 3)),
                    new GetPagedListRequestModel<AuditLogSortField>(
                        Page: 1, PageSize: 10, Sort: null, SortDirection: null, Search: null)));

            Assert.Equal(new[] { "3", "2" }, auditLogsPage.Items.Select(auditLog => auditLog.EntityId));
        }

        [Fact]
        public async Task AuditLogGetPagedListAsync_ReversedDateRange_ThrowsValidationException()
        {
            await Assert.ThrowsAsync<ValidationException>(() => _testHost.RunInNewScopeAsync<IAuditLogService, GetPagedListResponseModel<GetAuditLogResponseModel>>(
                auditLogService => auditLogService.GetPagedListAsync(
                    new GetAuditLogsBySearchFilterRequestModel(Action: null, TimeFromDate: new DateOnly(2030, 1, 5), TimeToDate: new DateOnly(2030, 1, 3)),
                    new GetPagedListRequestModel<AuditLogSortField>(
                        Page: 1, PageSize: 10, Sort: null, SortDirection: null, Search: null))));
        }

        [Fact]
        public async Task GetRoutesPagedListBySearchFilterAsync_StatusFilterAndPaging_ReturnsFilteredPage()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var firstDeparture = new DateTime(2030, 5, 1, 8, 0, 0);
            var seededRoutes = Enumerable.Range(0, 6)
                .Select(dayOffset => BusinessLogicTestHost.CreateRoute(auto, driver, firstDeparture.AddDays(dayOffset), firstDeparture.AddDays(dayOffset).AddHours(4),
                    dayOffset % 2 == 0 ? RouteStatus.Planned : RouteStatus.Cancelled))
                .ToArray<object>();
            await _testHost.SeedAsync(seededRoutes);

            var routesPage = await _testHost.RunInNewScopeAsync<IRouteService, GetPagedListResponseModel<GetRouteResponseModel>>(
                routeService => routeService.GetRoutesPagedListBySearchFilterAsync(
                    new GetRoutesBySearchFilterRequestModel(null, null, null, null, RouteStatus.Planned, null, null),
                    new GetPagedListRequestModel<RouteSortField>(Page: 1, PageSize: 2, Sort: RouteSortField.DepartureTime, SortDirection: SortDirection.Descending, Search: null)));

            Assert.Equal(3, routesPage.TotalCount);
            Assert.Equal(new[] { firstDeparture.AddDays(4), firstDeparture.AddDays(2) }, routesPage.Items.Select(route => route.DepartureTime));
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }

        private static async Task CreateUserInRoleAsync(UserManager<IdentityUser> userManager, string username, string roleName)
        {
            var user = new IdentityUser { UserName = username, Email = $"{username}@example.com" };
            Assert.True((await userManager.CreateAsync(user, "Password123!")).Succeeded);
            Assert.True((await userManager.AddToRoleAsync(user, roleName)).Succeeded);
        }
    }
}
