namespace business_logic.DTOs
{
    public abstract record SessionResponseModelBase
    {
        public string AccessToken { get; init; } = string.Empty;
        public DateTimeOffset AccessTokenExpiresAtUtc { get; init; }
        public string RefreshToken { get; init; } = string.Empty;
        public DateTimeOffset RefreshTokenExpiresAtUtc { get; init; }
        public string Username { get; init; } = string.Empty;
        public string? Email { get; init; }
        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
