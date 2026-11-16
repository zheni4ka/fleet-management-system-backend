namespace business_logic.DTOs
{
    public sealed record GetRoutesPeriodReportDriverSummaryResponseModel
    {
        public int DriverId { get; init; }
        public int RoutesCount { get; init; }
        public int CompletedRoutesCount { get; init; }
        public double CompletedRoutesDrivingHours { get; init; }
    }
}
