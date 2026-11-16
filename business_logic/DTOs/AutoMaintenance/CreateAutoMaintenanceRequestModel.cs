namespace business_logic.DTOs
{
    public sealed record CreateAutoMaintenanceRequestModel
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public DateTime ServiceDate { get; init; }
        public int AutoId { get; init; }
    }
}
