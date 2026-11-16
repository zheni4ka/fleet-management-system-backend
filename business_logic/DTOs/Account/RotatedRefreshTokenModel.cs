namespace business_logic.DTOs
{
    public sealed record RotatedRefreshTokenModel(string UserId, IssuedRefreshTokenModel IssuedRefreshToken);
}
