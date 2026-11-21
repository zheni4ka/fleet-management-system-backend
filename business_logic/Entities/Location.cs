using business_logic.Interfaces;

namespace business_logic.Entities
{
    public class Location : IArchivable
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string TimeZoneId { get; set; } = string.Empty;
        public int? GeoNamesId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public IEnumerable<Route> StartRoutes { get; set; }
        public IEnumerable<Route> DestinationRoutes { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ArchivedAtUtc { get; set; }
    }
}
