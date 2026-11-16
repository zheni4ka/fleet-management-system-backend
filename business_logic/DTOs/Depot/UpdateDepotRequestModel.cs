namespace business_logic.DTOs
{
    public sealed record UpdateDepotRequestModel : DepotRequestModelBase
    {
        public int Id { get; init; }
    }
}
