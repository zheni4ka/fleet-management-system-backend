using business_logic.Interfaces;

namespace business_logic.Entities
{
    public class Driver : IHasRowVersion, IBelongsToDepot, IArchivable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string LicenseNumber { get; set; }
        public IEnumerable<Route> Routes { get; set; }
        public int DepotId { get; set; }
        public Depot? Depot { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAtUtc { get; set; }
        public byte[] RowVersion { get; set; } = [];
    }
}
