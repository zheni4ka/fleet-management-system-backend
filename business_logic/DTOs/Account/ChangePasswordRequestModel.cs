namespace business_logic.DTOs
{
    public sealed record ChangePasswordRequestModel
    {
        public string Username { get; init; } = string.Empty;
        public string CurrentPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
    }
}
