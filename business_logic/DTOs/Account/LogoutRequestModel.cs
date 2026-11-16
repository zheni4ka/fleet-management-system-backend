namespace business_logic.DTOs
{
    public sealed record LogoutRequestModel
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}
