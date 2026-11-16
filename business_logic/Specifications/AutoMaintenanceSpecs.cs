using Ardalis.Specification;
using business_logic.Entities;

namespace business_logic.Specifications
{
    public class AutoMaintenanceSpecs
    {
        public class ById : Specification<AutoMaintenance>
        {
            public ById(int id)
            {
                Query.Where(am => am.Id == id);
            }
        }

        public class ByAutoId : Specification<AutoMaintenance>
        {
            public ByAutoId(int autoId)
            {
                Query.Where(am => am.AutoId == autoId);
            }
        }

        public class IncompleteDueByAutoId : Specification<AutoMaintenance>
        {
            public IncompleteDueByAutoId(int autoId, DateTime dueMaintenanceEndExclusiveDateTime)
            {
                Query.Where(am => am.AutoId == autoId
                    && !am.IsCompleted
                    && am.ServiceDate < dueMaintenanceEndExclusiveDateTime);
            }
        }

        public class IncompleteIdOverlappingRouteIntervalForAuto : Specification<AutoMaintenance, int?>
        {
            public IncompleteIdOverlappingRouteIntervalForAuto(int autoId, DateTime departure, DateTime arrival)
            {
                var departureDayStartDateTime = departure.Date;
                Query.Where(am => am.AutoId == autoId
                    && !am.IsCompleted
                    && am.ServiceDate >= departureDayStartDateTime
                    && am.ServiceDate < arrival)
                    .OrderBy(am => am.ServiceDate);
                Query.Select(am => (int?)am.Id);
            }
        }
    }
}
