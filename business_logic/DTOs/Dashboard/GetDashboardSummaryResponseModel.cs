namespace business_logic.DTOs
{
    public sealed record GetDashboardSummaryResponseModel(
        int AutosCount,
        int DriversCount,
        int RoutesCount,
        int ActiveRoutesCount);
}
