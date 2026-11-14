using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<AuditLogDTO>>> GetAll()
        {
            return Ok(await _auditLogService.GetLogsAsync());
        }

        [HttpGet("pagedList")]
        public async Task<ActionResult<GetPagedListResponseModel<AuditLogDTO>>> GetAuditLogsPagedList(
            [FromQuery] GetPagedListRequestModel<AuditLogSortField> pagedListRequest,
            [FromQuery] AuditAction? action)
        {
            return Ok(await _auditLogService.GetPagedListAsync(pagedListRequest, action));
        }
    }
}
