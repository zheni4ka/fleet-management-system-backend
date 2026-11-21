using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Helpers;
using data_access.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace data_access.data
{
    public static class DemoDataSeeder
    {
        private const int MinimumRouteDurationInHours = 4;
        private const int MaximumRouteDurationInHours = 13;
        private const int MinimumRestBetweenRoutesInHours = 60;
        private const int MaximumRestBetweenRoutesInHours = 180;
        private const int CompletedRouteCancellationPercent = 10;
        private const int PlannedRouteCancellationPercent = 5;

        public static async Task<DemoDataSeedSummary> SeedAsync(IServiceProvider services, DemoSeedSettings? demoSeedSettings, DateTime nowUtc)
        {
            var dispatcherPassword = demoSeedSettings?.DispatcherPassword;
            if (string.IsNullOrWhiteSpace(dispatcherPassword))
            {
                throw new InvalidOperationException(
                    $"'{DemoSeedSettings.SectionName}:{nameof(DemoSeedSettings.DispatcherPassword)}' is not configured, so the demo dispatchers cannot be created.");
            }

            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FMS_DbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            if (await dbContext.Set<Auto>().IgnoreQueryFilters().AnyAsync())
            {
                throw new InvalidOperationException("The database already contains vehicles; the demo seed runs only on an empty fleet.");
            }

            var adminUserId = (await userManager.GetUsersInRoleAsync(UserRoles.Admin)).OrderBy(adminUser => adminUser.UserName).FirstOrDefault()?.Id
                ?? throw new InvalidOperationException("No administrator exists yet; the demo seed needs one to attribute catalog changes to.");

            var random = new Random(DemoDataCatalog.RandomSeed);
            var businessToday = BusinessTimeZoneConverter.ConvertUtcToBusinessDateTime(nowUtc).Date;
            var historyStartUtc = BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(businessToday.AddDays(-DemoDataCatalog.HistoryDaysCount));
            var planningHorizonEndUtc = BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(businessToday.AddDays(DemoDataCatalog.PlanningHorizonDaysCount + 1));
            var catalogSetupUtc = historyStartUtc.AddDays(-1);
            var auditLogs = new List<AuditLog>();

            await using var demoSeedTransaction = await dbContext.Database.BeginTransactionAsync();

            await FillMissingCityCoordinatesAsync(dbContext);
            var depots = DemoDataCatalog.Depots.Select(depotDefinition => new Depot { Name = depotDefinition.Name }).ToList();
            dbContext.AddRange(depots);
            await dbContext.SaveChangesAsync();

            var autos = DemoDataCatalog.Autos.Select(autoDefinition => new Auto
            {
                Mark = autoDefinition.Mark,
                Model = autoDefinition.Model,
                Color = autoDefinition.Color,
                Number = autoDefinition.Number,
                Capacity = autoDefinition.Capacity,
                DepotId = depots[autoDefinition.DepotIndex].Id
            }).ToList();
            var drivers = DemoDataCatalog.Drivers.Select(driverDefinition => new Driver
            {
                Name = driverDefinition.Name,
                Surname = driverDefinition.Surname,
                Patronymic = driverDefinition.Patronymic,
                LicenseNumber = driverDefinition.LicenseNumber,
                DepotId = depots[driverDefinition.DepotIndex].Id
            }).ToList();
            dbContext.AddRange(autos);
            dbContext.AddRange(drivers);
            await dbContext.SaveChangesAsync();

            var dispatcherUserIdsByDepotId = await CreateDispatchersAsync(dbContext, userManager, depots, dispatcherPassword);
            auditLogs.AddRange(depots.Select(depot => CreateAuditLog(adminUserId, catalogSetupUtc, AuditAction.DepotCreated, depot.Id)));
            auditLogs.AddRange(autos.Select(auto => CreateAuditLog(adminUserId, catalogSetupUtc.AddMinutes(10), AuditAction.AutoCreated, auto.Id)));
            auditLogs.AddRange(drivers.Select(driver => CreateAuditLog(adminUserId, catalogSetupUtc.AddMinutes(20), AuditAction.DriverCreated, driver.Id)));
            auditLogs.AddRange(dispatcherUserIdsByDepotId.Values.Select(dispatcherUserId => CreateAuditLog(adminUserId, catalogSetupUtc.AddMinutes(30), AuditAction.UserRegistered, dispatcherUserId)));

            var maintenances = CreateMaintenances(autos, businessToday);
            dbContext.AddRange(maintenances);
            await dbContext.SaveChangesAsync();
            foreach (var maintenance in maintenances)
            {
                var maintenanceStartUtc = BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(maintenance.ServiceDate);
                auditLogs.Add(CreateAuditLog(adminUserId, Earliest(maintenanceStartUtc.AddDays(-1), nowUtc.AddHours(-1)), AuditAction.MaintenanceCreated, maintenance.Id));
                if (maintenance.IsCompleted && maintenance.PlannedEndDate.HasValue)
                {
                    auditLogs.Add(CreateAuditLog(adminUserId, BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(maintenance.PlannedEndDate.Value), AuditAction.MaintenanceUpdated, maintenance.Id,
                        [CreatePropertyChange(nameof(AutoMaintenance.IsCompleted), false, true)]));
                }
            }

            var routes = CreateRoutes(autos, drivers, depots, maintenances, historyStartUtc, planningHorizonEndUtc, nowUtc, random);
            dbContext.AddRange(routes);
            await dbContext.SaveChangesAsync();
            foreach (var route in routes)
            {
                auditLogs.AddRange(CreateRouteAuditLogs(route, dispatcherUserIdsByDepotId[route.DepotId], historyStartUtc, nowUtc, random));
            }

            foreach (var auto in autos)
            {
                auto.Status = GetDemoAutoStatus(auto, routes, maintenances, nowUtc);
            }

            dbContext.AddRange(auditLogs);
            await dbContext.SaveChangesAsync();
            await demoSeedTransaction.CommitAsync();

            return new DemoDataSeedSummary(
                DepotsCount: depots.Count,
                AutosCount: autos.Count,
                DriversCount: drivers.Count,
                DispatchersCount: dispatcherUserIdsByDepotId.Count,
                LocationsCount: DemoDataCatalog.CityCoordinates.Count,
                RoutesCount: routes.Count,
                MaintenancesCount: maintenances.Count,
                AuditLogsCount: auditLogs.Count);
        }

        private static async Task FillMissingCityCoordinatesAsync(FMS_DbContext dbContext)
        {
            var demoLocationIds = DemoDataCatalog.CityCoordinates.Select(cityCoordinates => cityCoordinates.LocationId).ToList();
            var demoLocations = await dbContext.Set<Location>().IgnoreQueryFilters()
                .Where(location => demoLocationIds.Contains(location.Id))
                .ToListAsync();
            var missingDemoLocationIds = demoLocationIds.Except(demoLocations.Select(location => location.Id)).ToList();
            if (missingDemoLocationIds.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Seeded locations '{string.Join(", ", missingDemoLocationIds)}' are missing; apply all migrations before the demo seed.");
            }

            foreach (var demoLocation in demoLocations.Where(location => !location.Latitude.HasValue || !location.Longitude.HasValue))
            {
                var cityCoordinates = DemoDataCatalog.CityCoordinates.Single(coordinates => coordinates.LocationId == demoLocation.Id);
                demoLocation.Latitude = cityCoordinates.Latitude;
                demoLocation.Longitude = cityCoordinates.Longitude;
            }
        }

        private static async Task<IReadOnlyDictionary<int, string>> CreateDispatchersAsync(
            FMS_DbContext dbContext, UserManager<IdentityUser> userManager, IReadOnlyList<Depot> depots, string dispatcherPassword)
        {
            var dispatcherUserIdsByDepotId = new Dictionary<int, string>();
            foreach (var dispatcherDefinition in DemoDataCatalog.Dispatchers)
            {
                var dispatcher = new IdentityUser { UserName = dispatcherDefinition.UserName, Email = dispatcherDefinition.Email, EmailConfirmed = true };
                EnsureIdentitySucceeded(await userManager.CreateAsync(dispatcher, dispatcherPassword), dispatcherDefinition.UserName);
                EnsureIdentitySucceeded(await userManager.AddToRoleAsync(dispatcher, UserRoles.Dispatcher), dispatcherDefinition.UserName);
                var dispatcherDepotId = depots[dispatcherDefinition.DepotIndex].Id;
                dbContext.Add(new UserDepot { UserId = dispatcher.Id, DepotId = dispatcherDepotId });
                dispatcherUserIdsByDepotId[dispatcherDepotId] = dispatcher.Id;
            }

            await dbContext.SaveChangesAsync();
            return dispatcherUserIdsByDepotId;
        }

        private static List<AutoMaintenance> CreateMaintenances(IReadOnlyList<Auto> autos, DateTime businessToday)
        {
            return DemoDataCatalog.Maintenances.Select(maintenanceDefinition => new AutoMaintenance
            {
                AutoId = autos[maintenanceDefinition.AutoIndex].Id,
                Name = maintenanceDefinition.Name,
                Description = maintenanceDefinition.Description,
                ServiceDate = businessToday.AddDays(maintenanceDefinition.StartDayOffset).AddHours(maintenanceDefinition.StartHour),
                PlannedEndDate = maintenanceDefinition.PlannedEndDayOffset.HasValue
                    ? businessToday.AddDays(maintenanceDefinition.PlannedEndDayOffset.Value).AddHours(maintenanceDefinition.PlannedEndHour)
                    : null,
                IsCompleted = maintenanceDefinition.IsCompleted
            }).ToList();
        }

        private static List<Route> CreateRoutes(
            IReadOnlyList<Auto> autos,
            IReadOnlyList<Driver> drivers,
            IReadOnlyList<Depot> depots,
            IReadOnlyList<AutoMaintenance> maintenances,
            DateTime historyStartUtc,
            DateTime planningHorizonEndUtc,
            DateTime nowUtc,
            Random random)
        {
            var routes = new List<Route>();
            var busyIntervalsByDriverId = drivers.ToDictionary(driver => driver.Id, _ => new List<(DateTime StartUtc, DateTime EndUtc)>());
            var cityLocationIds = DemoDataCatalog.CityCoordinates.Select(cityCoordinates => cityCoordinates.LocationId).ToList();

            var depotHomeLocationIdsByDepotId = depots
                .Select((depot, depotIndex) => (depot.Id, DemoDataCatalog.Depots[depotIndex].HomeLocationId))
                .ToDictionary(depotHomeLocation => depotHomeLocation.Id, depotHomeLocation => depotHomeLocation.HomeLocationId);

            foreach (var auto in autos)
            {
                var depotHomeLocationId = depotHomeLocationIdsByDepotId[auto.DepotId];
                var depotDriverIds = drivers.Where(driver => driver.DepotId == auto.DepotId).Select(driver => driver.Id).ToList();
                var autoMaintenanceIntervalsUtc = maintenances
                    .Where(maintenance => maintenance.AutoId == auto.Id)
                    .Select(maintenance => (
                        StartUtc: BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(maintenance.ServiceDate),
                        EndUtc: maintenance.PlannedEndDate.HasValue
                            ? BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(maintenance.PlannedEndDate.Value)
                            : DateTime.MaxValue))
                    .ToList();
                var nextDepartureUtc = RoundToHalfHour(historyStartUtc.AddHours(random.Next(0, 72)));

                while (nextDepartureUtc < planningHorizonEndUtc)
                {
                    var departureTimeUtc = nextDepartureUtc;
                    var arrivalTimeUtc = departureTimeUtc.AddHours(random.Next(MinimumRouteDurationInHours, MaximumRouteDurationInHours + 1));
                    var overlappingMaintenanceInterval = autoMaintenanceIntervalsUtc
                        .FirstOrDefault(maintenanceInterval => maintenanceInterval.StartUtc < arrivalTimeUtc && maintenanceInterval.EndUtc > departureTimeUtc);
                    if (overlappingMaintenanceInterval != default)
                    {
                        if (overlappingMaintenanceInterval.EndUtc == DateTime.MaxValue)
                        {
                            break;
                        }

                        nextDepartureUtc = RoundToHalfHour(overlappingMaintenanceInterval.EndUtc.AddHours(12));
                        continue;
                    }

                    var freeDriverId = FindFreeDriverId(depotDriverIds, busyIntervalsByDriverId, departureTimeUtc, arrivalTimeUtc, random);
                    if (!freeDriverId.HasValue)
                    {
                        nextDepartureUtc = departureTimeUtc.AddHours(6);
                        continue;
                    }

                    var startLocationId = random.Next(2) == 0 ? depotHomeLocationId : cityLocationIds[random.Next(cityLocationIds.Count)];
                    var destinationLocationId = PickDifferentLocationId(cityLocationIds, startLocationId, random);
                    routes.Add(CreateRoute(auto, freeDriverId.Value, startLocationId, destinationLocationId, departureTimeUtc, arrivalTimeUtc, nowUtc, random));
                    busyIntervalsByDriverId[freeDriverId.Value].Add((departureTimeUtc, arrivalTimeUtc));
                    nextDepartureUtc = RoundToHalfHour(arrivalTimeUtc.AddHours(random.Next(MinimumRestBetweenRoutesInHours, MaximumRestBetweenRoutesInHours + 1)));
                }
            }

            return routes;
        }

        private static Route CreateRoute(
            Auto auto, int driverId, int startLocationId, int destinationLocationId, DateTime departureTimeUtc, DateTime arrivalTimeUtc, DateTime nowUtc, Random random)
        {
            var route = new Route
            {
                AutoId = auto.Id,
                DriverId = driverId,
                DepotId = auto.DepotId,
                StartLocationId = startLocationId,
                DestinationLocationId = destinationLocationId,
                DepartureTimeUtc = departureTimeUtc,
                ArrivalTimeUtc = arrivalTimeUtc,
                Status = RouteStatus.Planned
            };

            if (arrivalTimeUtc <= nowUtc)
            {
                if (random.Next(100) < CompletedRouteCancellationPercent)
                {
                    route.Status = RouteStatus.Cancelled;
                    return route;
                }

                route.Status = RouteStatus.Completed;
                route.ActualDepartureTimeUtc = departureTimeUtc.AddMinutes(random.Next(-10, 26));
                route.ActualArrivalTimeUtc = Earliest(arrivalTimeUtc.AddMinutes(random.Next(-20, 61)), nowUtc);
                return route;
            }

            if (departureTimeUtc <= nowUtc)
            {
                route.Status = RouteStatus.InProgress;
                route.ActualDepartureTimeUtc = Earliest(departureTimeUtc.AddMinutes(random.Next(0, 16)), nowUtc);
                return route;
            }

            if (random.Next(100) < PlannedRouteCancellationPercent)
            {
                route.Status = RouteStatus.Cancelled;
            }

            return route;
        }

        private static IEnumerable<AuditLog> CreateRouteAuditLogs(Route route, string dispatcherUserId, DateTime historyStartUtc, DateTime nowUtc, Random random)
        {
            var routeCreatedUtc = Latest(
                historyStartUtc.AddDays(-1),
                Earliest(route.DepartureTimeUtc.AddHours(-random.Next(24, 120)), nowUtc.AddHours(-random.Next(1, 48))));
            yield return CreateAuditLog(dispatcherUserId, routeCreatedUtc, AuditAction.RouteCreated, route.Id);

            if (route.Status == RouteStatus.Cancelled)
            {
                var routeCancelledUtc = Earliest(routeCreatedUtc.AddHours(random.Next(2, 20)), Earliest(route.DepartureTimeUtc, nowUtc));
                yield return CreateAuditLog(dispatcherUserId, routeCancelledUtc, AuditAction.RouteUpdated, route.Id,
                    [CreatePropertyChange(nameof(Route.Status), RouteStatus.Planned, RouteStatus.Cancelled)]);
                yield break;
            }

            if (route.ActualDepartureTimeUtc.HasValue)
            {
                yield return CreateAuditLog(dispatcherUserId, route.ActualDepartureTimeUtc.Value, AuditAction.RouteUpdated, route.Id,
                [
                    CreatePropertyChange(nameof(Route.ActualDepartureTimeUtc), null, route.ActualDepartureTimeUtc.Value),
                    CreatePropertyChange(nameof(Route.Status), RouteStatus.Planned, RouteStatus.InProgress)
                ]);
            }

            if (route.ActualArrivalTimeUtc.HasValue)
            {
                yield return CreateAuditLog(dispatcherUserId, route.ActualArrivalTimeUtc.Value, AuditAction.RouteUpdated, route.Id,
                [
                    CreatePropertyChange(nameof(Route.ActualArrivalTimeUtc), null, route.ActualArrivalTimeUtc.Value),
                    CreatePropertyChange(nameof(Route.Status), RouteStatus.InProgress, RouteStatus.Completed)
                ]);
            }
        }

        private static AutoStatus GetDemoAutoStatus(Auto auto, IReadOnlyList<Route> routes, IReadOnlyList<AutoMaintenance> maintenances, DateTime nowUtc)
        {
            if (routes.Any(route => route.AutoId == auto.Id && route.Status == RouteStatus.InProgress))
            {
                return AutoStatus.InService;
            }

            var hasStartedIncompleteMaintenance = maintenances.Any(maintenance => maintenance.AutoId == auto.Id
                && !maintenance.IsCompleted
                && BusinessTimeZoneConverter.ConvertBusinessDateTimeToUtc(maintenance.ServiceDate) <= nowUtc);
            return hasStartedIncompleteMaintenance ? AutoStatus.UnderMaintenance : AutoStatus.Available;
        }

        private static int? FindFreeDriverId(
            IReadOnlyList<int> depotDriverIds,
            IReadOnlyDictionary<int, List<(DateTime StartUtc, DateTime EndUtc)>> busyIntervalsByDriverId,
            DateTime departureTimeUtc,
            DateTime arrivalTimeUtc,
            Random random)
        {
            var firstCandidateIndex = random.Next(depotDriverIds.Count);
            for (var candidateOffset = 0; candidateOffset < depotDriverIds.Count; candidateOffset++)
            {
                var candidateDriverId = depotDriverIds[(firstCandidateIndex + candidateOffset) % depotDriverIds.Count];
                var isCandidateDriverBusy = busyIntervalsByDriverId[candidateDriverId]
                    .Any(busyInterval => busyInterval.StartUtc < arrivalTimeUtc && busyInterval.EndUtc > departureTimeUtc);
                if (!isCandidateDriverBusy)
                {
                    return candidateDriverId;
                }
            }

            return null;
        }

        private static int PickDifferentLocationId(IReadOnlyList<int> cityLocationIds, int excludedLocationId, Random random)
        {
            var candidateLocationIds = cityLocationIds.Where(locationId => locationId != excludedLocationId).ToList();
            return candidateLocationIds[random.Next(candidateLocationIds.Count)];
        }

        private static AuditLog CreateAuditLog(string userId, DateTime timeUtc, AuditAction auditAction, object entityId, IReadOnlyList<AuditLogPropertyChangeModel>? propertyChanges = null)
        {
            return new AuditLog
            {
                DispatcherId = userId,
                Time = timeUtc,
                Action = auditAction,
                EntityId = Convert.ToString(entityId, CultureInfo.InvariantCulture),
                ChangesJson = propertyChanges is null ? null : AuditLogChangesSerializer.SerializeAuditLogChanges(propertyChanges)
            };
        }

        private static AuditLogPropertyChangeModel CreatePropertyChange(string propertyName, object? previousValue, object currentValue)
        {
            return new AuditLogPropertyChangeModel(
                PropertyName: propertyName,
                PreviousValue: FormatAuditedValue(previousValue),
                CurrentValue: FormatAuditedValue(currentValue));
        }

        private static string? FormatAuditedValue(object? auditedValue)
        {
            return auditedValue switch
            {
                null => null,
                DateTime auditedDateTime => auditedDateTime.ToString("O", CultureInfo.InvariantCulture),
                IFormattable auditedFormattableValue => auditedFormattableValue.ToString(null, CultureInfo.InvariantCulture),
                _ => auditedValue.ToString()
            };
        }

        private static void EnsureIdentitySucceeded(IdentityResult identityResult, string userName)
        {
            if (!identityResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Demo dispatcher '{userName}' was not created: {string.Join(" ", identityResult.Errors.Select(identityError => identityError.Description))}");
            }
        }

        private static DateTime RoundToHalfHour(DateTime utcDateTime)
        {
            var halfHourTicks = TimeSpan.FromMinutes(30).Ticks;
            return new DateTime(utcDateTime.Ticks / halfHourTicks * halfHourTicks, DateTimeKind.Utc);
        }

        private static DateTime Earliest(DateTime firstUtcDateTime, DateTime secondUtcDateTime)
        {
            return firstUtcDateTime < secondUtcDateTime ? firstUtcDateTime : secondUtcDateTime;
        }

        private static DateTime Latest(DateTime firstUtcDateTime, DateTime secondUtcDateTime)
        {
            return firstUtcDateTime > secondUtcDateTime ? firstUtcDateTime : secondUtcDateTime;
        }
    }
}
