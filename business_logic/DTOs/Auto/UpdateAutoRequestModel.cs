namespace business_logic.DTOs
{
    public sealed record UpdateAutoRequestModel : AutoRequestModelBase
    {
        public int Id { get; init; }
        public byte[] RowVersion { get; init; } = [];
    }
}
