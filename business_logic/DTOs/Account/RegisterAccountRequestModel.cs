namespace business_logic.DTOs
{
    public sealed record RegisterAccountRequestModel
    {
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }
}
