

using business_logic.Entities;

namespace business_logic.DTOs
{
    public class AutoDTO
    {
        public int Id { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string Number { get; set; }
        public double Capacity { get; set; }
        public AutoStatus Status { get; set; }
    }
}
