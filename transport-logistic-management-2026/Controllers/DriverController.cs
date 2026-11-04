using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<DriverDTO>>> GetAll()
        {
            return Ok(await _driverService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DriverDTO>> GetById([FromRoute] int id)
        {
            return Ok(await _driverService.GetAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<DriverDTO>> Create([FromBody] CreateDriverModel model)
        {
            var driver = await _driverService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = driver.Id }, driver);
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Edit([FromBody] EditDriverModel model)
        {
            await _driverService.EditAsync(model);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _driverService.DeleteAsync(id);
            return NoContent();
        }
    }
}
