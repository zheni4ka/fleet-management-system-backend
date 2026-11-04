using Ardalis.Specification;
using business_logic.Entities;

namespace business_logic.Specifications
{
    public class AuditLogSpecs
    {
        public class ById : Specification<AuditLog>
        {
            public ById(int id)
            {
                Query.Where(x => x.Id == id);
            }
        }

        public class All : Specification<AuditLog>
        {
            public All()
            {
                Query.OrderByDescending(x => x.Time);
            }
        }
    }
}
