using business_logic.Entities;

namespace business_logic.Constants
{
    public static class RouteStatusGroups
    {
        public static readonly IReadOnlyList<RouteStatus> Active = new[] { RouteStatus.Planned, RouteStatus.InProgress };
    }
}
