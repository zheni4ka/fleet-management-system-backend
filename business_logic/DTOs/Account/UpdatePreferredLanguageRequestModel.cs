namespace business_logic.DTOs
{
    public sealed record UpdatePreferredLanguageRequestModel
    {
        public string? Language { get; init; }
    }
}
