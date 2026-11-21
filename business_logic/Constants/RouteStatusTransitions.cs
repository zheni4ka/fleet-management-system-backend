using business_logic.Entities;

namespace business_logic.Constants
{
    public static class RouteStatusTransitions
    {
        public static readonly IReadOnlyDictionary<RouteStatus, IReadOnlyList<RouteStatus>> AllowedTargetStatusesBySourceStatus =
            new Dictionary<RouteStatus, IReadOnlyList<RouteStatus>>
            {
                [RouteStatus.Planned] = [RouteStatus.InProgress, RouteStatus.Cancelled],
                [RouteStatus.InProgress] = [RouteStatus.Completed, RouteStatus.Cancelled],
                [RouteStatus.Completed] = [],
                [RouteStatus.Cancelled] = []
            };
    }
}
