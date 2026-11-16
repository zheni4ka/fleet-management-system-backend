using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed record GetAutoResponseModel
    {
        public int Id { get; init; }
        public string Mark { get; init; }
        public string Model { get; init; }
        public string Color { get; init; }
        public string Number { get; init; }
        public double Capacity { get; init; }
        public AutoStatus Status { get; init; }
        public int DepotId { get; init; }
        public string DepotName { get; init; } = string.Empty;
        public bool IsArchived { get; init; }
        public byte[] RowVersion { get; init; } = [];
    }
}
