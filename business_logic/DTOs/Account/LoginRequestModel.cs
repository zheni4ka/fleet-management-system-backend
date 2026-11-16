namespace business_logic.DTOs
{
    public sealed record LoginRequestModel
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
