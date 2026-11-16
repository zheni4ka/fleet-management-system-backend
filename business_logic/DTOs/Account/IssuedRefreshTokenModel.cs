namespace business_logic.DTOs
{
    public sealed record IssuedRefreshTokenModel(string RefreshToken, DateTimeOffset ExpiresAtUtc);
}
