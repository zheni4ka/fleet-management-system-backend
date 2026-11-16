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
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("getLocationsPagedList")]
        public async Task<ActionResult<GetPagedListResponseModel<GetLocationResponseModel>>> GetLocationsPagedList(
            [FromQuery] GetLocationsBySearchFilterRequestModel searchFilter,
            [FromQuery] GetPagedListRequestModel<LocationSortField> pagedListRequest)
        {
            return Ok(await _locationService.GetPagedListAsync(searchFilter, pagedListRequest));
        }

        [HttpGet("getLocationLookupOptions")]
        public async Task<ActionResult<IReadOnlyList<GetLocationLookupOptionResponseModel>>> GetLocationLookupOptions()
        {
            return Ok(await _locationService.GetLookupOptionsAsync());
        }

        [HttpGet("getLocationById/{id:int}")]
        public async Task<ActionResult<GetLocationResponseModel>> GetLocationById([FromRoute] int id)
        {
            return Ok(await _locationService.GetAsync(id));
        }

        [HttpPost("createLocation")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<GetLocationResponseModel>> CreateLocation([FromBody] CreateLocationRequestModel model)
        {
            var location = await _locationService.CreateAsync(model);
            return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, location);
        }

        [HttpDelete("deleteLocation/{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteLocation([FromRoute] int id)
        {
            await _locationService.DeleteAsync(id);
            return NoContent();
        }
    }
}
