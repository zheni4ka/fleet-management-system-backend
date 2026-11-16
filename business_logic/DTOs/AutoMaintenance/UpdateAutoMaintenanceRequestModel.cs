namespace business_logic.DTOs
{
    public sealed record UpdateAutoMaintenanceRequestModel
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public DateTime ServiceDate { get; init; }
        public int AutoId { get; init; }
        public bool IsCompleted { get; init; }
    }
}
