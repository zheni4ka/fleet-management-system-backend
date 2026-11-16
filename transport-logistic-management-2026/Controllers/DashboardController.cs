using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("getDashboardSummary")]
        public async Task<ActionResult<GetDashboardSummaryResponseModel>> GetDashboardSummary()
        {
            return Ok(await _dashboardService.GetDashboardSummaryAsync());
        }
    }
}
