namespace business_logic.DTOs
{
    public sealed record GetRoutesPeriodReportResponseModel(
        DateOnly PeriodStartDate,
        DateOnly PeriodEndDate,
        int TotalRoutesCount,
        int PlannedRoutesCount,
        int InProgressRoutesCount,
        int CompletedRoutesCount,
        int CancelledRoutesCount,
        double CompletedRoutesDrivingHours,
        int InvolvedAutosCount,
        int InvolvedDriversCount,
        IReadOnlyList<GetRoutesPeriodReportDriverSummaryResponseModel> DriverSummaries,
        IReadOnlyList<GetRouteResponseModel> Routes);
}
