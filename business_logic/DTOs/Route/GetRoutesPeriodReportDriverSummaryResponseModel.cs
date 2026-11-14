namespace business_logic.DTOs
{
    public sealed record GetRoutesPeriodReportDriverSummaryResponseModel(
        int DriverId,
        int RoutesCount,
        int CompletedRoutesCount,
        double CompletedRoutesDrivingHours);
}
