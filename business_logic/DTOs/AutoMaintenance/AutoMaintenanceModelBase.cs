namespace business_logic.DTOs
{
    public abstract record AutoMaintenanceModelBase
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public DateTime ServiceDate { get; init; }
        public int AutoId { get; init; }
    }
}
