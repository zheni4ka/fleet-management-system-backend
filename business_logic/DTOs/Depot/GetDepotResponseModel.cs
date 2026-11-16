namespace business_logic.DTOs
{
    public sealed record GetDepotResponseModel
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
