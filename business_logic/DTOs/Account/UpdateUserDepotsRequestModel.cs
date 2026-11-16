namespace business_logic.DTOs
{
    public sealed record UpdateUserDepotsRequestModel
    {
        public string UserId { get; init; } = string.Empty;
        public IReadOnlyList<int> DepotIds { get; init; } = [];
    }
}
