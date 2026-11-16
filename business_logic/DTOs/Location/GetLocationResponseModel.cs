namespace business_logic.DTOs
{
    public sealed record GetLocationResponseModel
    {
        public int Id { get; init; }
        public string City { get; init; }
        public string Country { get; init; }
        public string TimeZoneId { get; init; } = string.Empty;
        public bool IsArchived { get; init; }
    }
}
