namespace business_logic.DTOs
{
    public sealed record CreateAutoRequestModel
    {
        public string Mark { get; init; }
        public string Model { get; init; }
        public string Color { get; init; }
        public string Number { get; init; }
        public double Capacity { get; init; }
    }
}
