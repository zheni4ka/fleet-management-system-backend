namespace business_logic.DTOs
{
    public sealed record RefreshSessionRequestModel
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}
