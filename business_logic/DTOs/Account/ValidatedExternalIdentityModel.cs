namespace business_logic.DTOs
{
    public sealed record ValidatedExternalIdentityModel(
        string SubjectId,
        string? PreferredUsername,
        string? Email,
        IReadOnlyList<string> GroupIds);
}
