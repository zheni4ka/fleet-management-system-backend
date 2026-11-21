namespace business_logic.DTOs
{
    public sealed record GetAddressCandidateResponseModel
    {
        public string AddressLine { get; init; } = string.Empty;
        public string? District { get; init; }
        public string? Postcode { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }
}
