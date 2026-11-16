namespace business_logic.DTOs
{
    public abstract record ExistingAutoMaintenanceModelBase : AutoMaintenanceModelBase
    {
        public int Id { get; init; }
        public bool IsCompleted { get; init; }
        public byte[] RowVersion { get; init; } = [];
    }
}
