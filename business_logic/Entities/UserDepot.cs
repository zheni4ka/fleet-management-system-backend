using Microsoft.AspNetCore.Identity;

namespace business_logic.Entities
{
    public class UserDepot
    {
        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }
        public int DepotId { get; set; }
        public Depot? Depot { get; set; }
    }
}
