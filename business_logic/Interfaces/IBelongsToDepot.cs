using business_logic.Entities;

namespace business_logic.Interfaces
{
    public interface IBelongsToDepot
    {
        int DepotId { get; set; }
        Depot? Depot { get; set; }
    }
}
