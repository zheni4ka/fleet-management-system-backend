namespace business_logic.DTOs
{
    public abstract record AutoRequestModelBase
    {
        public string Mark { get; init; }
        public string Model { get; init; }
        public string Color { get; init; }
        public string Number { get; init; }
        public double Capacity { get; init; }
        public int DepotId { get; init; }
    }
}
