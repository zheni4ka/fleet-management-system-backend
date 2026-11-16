namespace business_logic.DTOs
{
    public sealed record ResetUserPasswordRequestModel
    {
        public string UserId { get; init; } = string.Empty;
    }
}
