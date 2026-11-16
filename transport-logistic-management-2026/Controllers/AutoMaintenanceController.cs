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
    [Authorize]
    public class AutoMaintenanceController : ControllerBase
    {
        private readonly IAutoMaintenanceService _maintenanceService;

        public AutoMaintenanceController(IAutoMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpGet("getAutoMaintenanceById/{id:int}")]
        public async Task<ActionResult<GetAutoMaintenanceResponseModel>> GetAutoMaintenanceById([FromRoute] int id)
        {
            return Ok(await _maintenanceService.GetAsync(id));
        }

        [HttpGet("getAutoMaintenancesByAutoId/{autoId:int}")]
        public async Task<ActionResult<IEnumerable<GetAutoMaintenanceResponseModel>>> GetAutoMaintenancesByAutoId([FromRoute] int autoId)
        {
            return Ok(await _maintenanceService.GetByAutoIdAsync(autoId));
        }

        [HttpPost("createAutoMaintenance")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<GetAutoMaintenanceResponseModel>> CreateAutoMaintenance([FromBody] CreateAutoMaintenanceRequestModel model)
        {
            var maintenance = await _maintenanceService.CreateAsync(model);
            return CreatedAtAction(nameof(GetAutoMaintenanceById), new { id = maintenance.Id }, maintenance);
        }

        [HttpPut("updateAutoMaintenance")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UpdateAutoMaintenance([FromBody] UpdateAutoMaintenanceRequestModel model)
        {
            await _maintenanceService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("deleteAutoMaintenance/{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteAutoMaintenance([FromRoute] int id)
        {
            await _maintenanceService.DeleteAsync(id);
            return NoContent();
        }
    }
}
