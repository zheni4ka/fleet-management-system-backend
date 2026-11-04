namespace business_logic.Entities
{
    public enum AuditAction
    {
        RouteCreated,
        RouteUpdated,
        RouteDeleted,

        AutoCreated,
        AutoUpdated,
        AutoStatusChanged,
        AutoDeleted,

        DriverCreated,
        DriverUpdated,
        DriverDeleted,

        LocationCreated,
        LocationDeleted,

        MaintenanceCreated,
        MaintenanceUpdated,
        MaintenanceDeleted,

        UserRegistered,
        UserDeleted
    }
}
