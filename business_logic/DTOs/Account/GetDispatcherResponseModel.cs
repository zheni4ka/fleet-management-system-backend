namespace business_logic.DTOs
{
    public sealed record GetDispatcherResponseModel
    {
        public string Id { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string? Email { get; init; }
        public bool IsDeactivated { get; init; }
        public IReadOnlyList<int> DepotIds { get; init; } = [];
    }
}
