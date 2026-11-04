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
    public class AutoController : ControllerBase
    {
        private readonly IAutoService _autoService;

        public AutoController(IAutoService autoService)
        {
            _autoService = autoService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<AutoDTO>>> GetAll()
        {
            return Ok(await _autoService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AutoDTO>> GetById([FromRoute] int id)
        {
            return Ok(await _autoService.GetAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<AutoDTO>> Create([FromBody] CreateAutoModel model)
        {
            var auto = await _autoService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = auto.Id }, auto);
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Edit([FromBody] EditAutoModel model)
        {
            await _autoService.EditAsync(model);
            return NoContent();
        }

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromBody] AutoStatus newStatus)
        {
            await _autoService.UpdateStatusAsync(id, newStatus);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _autoService.DeleteAsync(id);
            return NoContent();
        }
    }
}
