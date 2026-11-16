namespace business_logic.Entities
{
    public class Auto
    {
        public int Id { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string Number { get; set; }
        public double Capacity { get; set; }
        public IEnumerable<Route> Routes { get; set; }
        public IEnumerable<AutoMaintenance> Services { get; set; }
        public AutoStatus Status { get; set; } = AutoStatus.Available;
    }
}
