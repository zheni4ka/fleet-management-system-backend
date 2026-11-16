namespace business_logic.DTOs
{
    public sealed record GetLocationLookupOptionResponseModel
    {
        public int Id { get; init; }
        public string City { get; init; }
        public string Country { get; init; }
        public string TimeZoneId { get; init; } = string.Empty;
    }
}
