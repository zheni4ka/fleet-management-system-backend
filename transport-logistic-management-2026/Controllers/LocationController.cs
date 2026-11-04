using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<LocationDTO>>> GetAll()
        {
            return Ok(await _locationService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LocationDTO>> GetById([FromRoute] int id)
        {
            return Ok(await _locationService.GetAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<LocationDTO>> Create([FromBody] CreateLocationModel model)
        {
            var location = await _locationService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = location.Id }, location);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _locationService.DeleteAsync(id);
            return NoContent();
        }
    }
}
