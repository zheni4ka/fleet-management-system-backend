using business_logic.Interfaces;

namespace business_logic.Entities
{
    public class Auto : IHasRowVersion, IBelongsToDepot, IArchivable
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
        public int DepotId { get; set; }
        public Depot? Depot { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAtUtc { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
