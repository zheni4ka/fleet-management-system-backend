namespace business_logic.DTOs
{
    public sealed record LoginWithExternalIdentityRequestModel
    {
        public string IdToken { get; init; } = string.Empty;
        public string Nonce { get; init; } = string.Empty;
    }
}
