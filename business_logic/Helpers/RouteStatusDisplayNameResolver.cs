using business_logic.Constants;
using business_logic.Entities;

namespace business_logic.Helpers
{
    public static class RouteStatusDisplayNameResolver
    {
        public static string GetRouteStatusDisplayName(RouteStatus routeStatus)
        {
            return routeStatus switch
            {
                RouteStatus.Planned => EntityDisplayNames.RouteStatusPlanned,
                RouteStatus.InProgress => EntityDisplayNames.RouteStatusInProgress,
                RouteStatus.Completed => EntityDisplayNames.RouteStatusCompleted,
                RouteStatus.Cancelled => EntityDisplayNames.RouteStatusCancelled,
                _ => throw new InvalidOperationException($"Route status '{routeStatus}' has no display name.")
            };
        }
    }
}
