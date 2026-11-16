namespace business_logic.DTOs
{
    public sealed record CreateLocationRequestModel
    {
        public string City { get; init; }
        public string Country { get; init; }
        public string? TimeZoneId { get; init; }
    }
}
