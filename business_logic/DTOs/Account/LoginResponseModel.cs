namespace business_logic.DTOs
{
    public sealed record LoginResponseModel(
        string Token,
        string? Username,
        string? Email,
        IReadOnlyList<string> Roles);
}
