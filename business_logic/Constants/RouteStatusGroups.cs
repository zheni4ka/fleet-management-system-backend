using business_logic.Entities;

namespace business_logic.Constants
{
    public static class RouteStatusGroups
    {
        public static readonly IReadOnlyList<RouteStatus> Active = new[] { RouteStatus.Planned, RouteStatus.InProgress };
        public static readonly IReadOnlyList<RouteStatus> Final = new[] { RouteStatus.Completed, RouteStatus.Cancelled };
    }
}
