using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RouteController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RouteDTO>>> GetAll()
        {
            return Ok(await _routeService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<RouteDTO>> GetById([FromRoute] int id)
        {
            return Ok(await _routeService.GetAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<ActionResult<RouteDTO>> Create([FromBody] CreateRouteModel model)
        {
            var route = await _routeService.CreateAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = route.Id }, route);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] EditRouteModel model)
        {
            if (id != model.Id)
            {
                return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Route id in URL and body do not match." });
            }

            await _routeService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _routeService.DeleteAsync(id);
            return NoContent();
        }
    }
}
