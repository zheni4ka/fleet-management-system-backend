using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using transport_logistic_management_2026.Constants;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [ApiVersion(ApiVersions.Version1)]
    [Route(ApiVersions.VersionedControllerRoute)]
    [Authorize(Roles = UserRoles.Admin)]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet("getAuditLogsPagedList")]
        public async Task<ActionResult<GetPagedListResponseModel<GetAuditLogResponseModel>>> GetAuditLogsPagedList(
            [FromQuery] GetAuditLogsBySearchFilterRequestModel searchFilter,
            [FromQuery] GetPagedListRequestModel<AuditLogSortField> pagedListRequest)
        {
            return Ok(await _auditLogService.GetPagedListAsync(searchFilter, pagedListRequest));
        }
    }
}
