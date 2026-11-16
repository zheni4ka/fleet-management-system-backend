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
    public class RouteController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RouteController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet("getRoutesPagedList")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<ActionResult<GetPagedListResponseModel<GetRouteResponseModel>>> GetRoutesPagedList(
            [FromQuery] GetRoutesBySearchFilterRequestModel searchFilter,
            [FromQuery] GetPagedListRequestModel<RouteSortField> pagedListRequest)
        {
            return Ok(await _routeService.GetRoutesPagedListBySearchFilterAsync(searchFilter, pagedListRequest));
        }

        [HttpGet("getRoutesPeriodReport")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<ActionResult<GetRoutesPeriodReportResponseModel>> GetRoutesPeriodReport(
            [FromQuery] GetRoutesPeriodReportRequestModel periodReportRequest)
        {
            return Ok(await _routeService.GetRoutesPeriodReportAsync(periodReportRequest));
        }

        [HttpGet("getRouteById/{id:int}")]
        public async Task<ActionResult<GetRouteResponseModel>> GetRouteById([FromRoute] int id)
        {
            return Ok(await _routeService.GetAsync(id));
        }

        [HttpPost("createRoute")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<ActionResult<GetRouteResponseModel>> CreateRoute([FromBody] CreateRouteRequestModel model)
        {
            var route = await _routeService.CreateAsync(model);
            return CreatedAtAction(nameof(GetRouteById), new { id = route.Id }, route);
        }

        [HttpPut("updateRoute")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<IActionResult> UpdateRoute([FromBody] UpdateRouteRequestModel model)
        {
            await _routeService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("deleteRoute/{id:int}")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<IActionResult> DeleteRoute([FromRoute] int id)
        {
            await _routeService.DeleteAsync(id);
            return NoContent();
        }
    }
}
