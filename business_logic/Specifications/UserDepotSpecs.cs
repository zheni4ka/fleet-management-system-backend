using Ardalis.Specification;
using business_logic.Entities;

namespace business_logic.Specifications
{
    public class UserDepotSpecs
    {
        public class DepotIdsByUserId : Specification<UserDepot, int>
        {
            public DepotIdsByUserId(string userId)
            {
                Query.Where(userDepot => userDepot.UserId == userId);
                Query.Select(userDepot => userDepot.DepotId);
            }
        }

        public class ByUserId : Specification<UserDepot>
        {
            public ByUserId(string userId)
            {
                Query.Where(userDepot => userDepot.UserId == userId);
            }
        }

        public class AllUserDepots : Specification<UserDepot, UserDepot>
        {
            public AllUserDepots()
            {
                Query.Select(userDepot => userDepot);
            }
        }

        public class ByDepotId : Specification<UserDepot>
        {
            public ByDepotId(int depotId)
            {
                Query.Where(userDepot => userDepot.DepotId == depotId);
            }
        }
    }
}
