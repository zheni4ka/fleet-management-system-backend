namespace business_logic.DTOs
{
    public sealed record GetRoutesPeriodReportRequestModel(
        DateOnly PeriodStartDate,
        DateOnly PeriodEndDate);
}
