using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutoMaintenanceController : ControllerBase
    {
        private readonly IAutoMaintenanceService _maintenanceService;

        public AutoMaintenanceController(IAutoMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<AutoMaintenanceDTO>>> GetAll()
        {
            return Ok(await _maintenanceService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AutoMaintenanceDTO>> GetById([FromRoute] int id)
        {
            return Ok(await _maintenanceService.GetAsync(id));
        }

        [HttpGet("auto/{id:int}")]
        public async Task<ActionResult<IEnumerable<AutoMaintenanceDTO>>> GetByAutoId([FromRoute] int id)
        {
            return Ok(await _maintenanceService.GetByAutoIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<AutoMaintenanceDTO>> Create([FromBody] CreateAutoMaintenanceModel model)
        {
            var maintenance = await _maintenanceService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = maintenance.Id }, maintenance);
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Edit([FromBody] EditAutoMaintenanceModel model)
        {
            await _maintenanceService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _maintenanceService.DeleteAsync(id);
            return NoContent();
        }
    }
}
