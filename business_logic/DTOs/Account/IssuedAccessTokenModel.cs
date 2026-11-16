namespace business_logic.DTOs
{
    public sealed record IssuedAccessTokenModel(string AccessToken, DateTimeOffset ExpiresAtUtc);
}
