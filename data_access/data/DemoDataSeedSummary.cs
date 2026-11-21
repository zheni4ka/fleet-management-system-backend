namespace data_access.data
{
    public sealed record DemoDataSeedSummary(
        int DepotsCount,
        int AutosCount,
        int DriversCount,
        int DispatchersCount,
        int LocationsCount,
        int RoutesCount,
        int DriverTripsCount,
        int MaintenancesCount,
        int AuditLogsCount);
}
