using business_logic.Entities;

namespace business_logic.DTOs
{
    public sealed record GetAutoLookupOptionResponseModel
    {
        public int Id { get; init; }
        public string Mark { get; init; }
        public string Model { get; init; }
        public string Number { get; init; }
        public AutoStatus Status { get; init; }
    }
}
