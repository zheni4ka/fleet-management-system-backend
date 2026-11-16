namespace business_logic.DTOs
{
    public sealed record GetRoutesPeriodReportDriverSummaryResponseModel
    {
        public int DriverId { get; init; }
        public string DriverName { get; init; }
        public string DriverSurname { get; init; }
        public int RoutesCount { get; init; }
        public int CompletedRoutesCount { get; init; }
        public double CompletedRoutesDrivingHours { get; init; }
    }
}
