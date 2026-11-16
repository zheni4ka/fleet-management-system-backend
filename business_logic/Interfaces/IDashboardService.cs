using business_logic.DTOs;

namespace business_logic.Interfaces
{
    public interface IDashboardService
    {
        Task<GetDashboardSummaryResponseModel> GetDashboardSummaryAsync();
    }
}
