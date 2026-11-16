namespace business_logic.DTOs
{
    public sealed record UpdateDriverRequestModel : DriverRequestModelBase
    {
        public int Id { get; init; }
        public byte[] RowVersion { get; init; } = [];
    }
}
