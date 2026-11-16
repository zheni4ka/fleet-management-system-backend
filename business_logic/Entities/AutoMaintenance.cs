using business_logic.Interfaces;

namespace business_logic.Entities
{
    public class AutoMaintenance : IHasRowVersion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime ServiceDate { get; set; }
        public int AutoId { get; set; }
        public Auto Auto { get; set; }
        public bool IsCompleted { get; set; } = false;
        public byte[] RowVersion { get; set; } = [];
    }
}
