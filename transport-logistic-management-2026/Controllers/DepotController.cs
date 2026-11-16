using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepotController : ControllerBase
    {
        private readonly IDepotService _depotService;

        public DepotController(IDepotService depotService)
        {
            _depotService = depotService;
        }

        [HttpGet("getDepots")]
        public async Task<ActionResult<IReadOnlyList<GetDepotResponseModel>>> GetDepots()
        {
            return Ok(await _depotService.GetDepotsAsync());
        }

        [HttpPost("createDepot")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<GetDepotResponseModel>> CreateDepot([FromBody] CreateDepotRequestModel model)
        {
            return Ok(await _depotService.CreateAsync(model));
        }

        [HttpPut("updateDepot")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UpdateDepot([FromBody] UpdateDepotRequestModel model)
        {
            await _depotService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("deleteDepot/{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteDepot([FromRoute] int id)
        {
            await _depotService.DeleteAsync(id);
            return NoContent();
        }
    }
}
