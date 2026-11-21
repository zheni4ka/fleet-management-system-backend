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
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;
        private readonly IDriverTripService _driverTripService;

        public DriverController(IDriverService driverService, IDriverTripService driverTripService)
        {
            _driverService = driverService;
            _driverTripService = driverTripService;
        }

        [HttpGet("getDriversPagedList")]
        public async Task<ActionResult<GetPagedListResponseModel<GetDriverResponseModel>>> GetDriversPagedList(
            [FromQuery] GetDriversBySearchFilterRequestModel searchFilter,
            [FromQuery] GetPagedListRequestModel<DriverSortField> pagedListRequest)
        {
            return Ok(await _driverService.GetPagedListAsync(searchFilter, pagedListRequest));
        }

        [HttpGet("getDriverLookupOptions")]
        public async Task<ActionResult<IReadOnlyList<GetDriverLookupOptionResponseModel>>> GetDriverLookupOptions()
        {
            return Ok(await _driverService.GetLookupOptionsAsync());
        }

        [HttpGet("getDriverById/{id:int}")]
        public async Task<ActionResult<GetDriverResponseModel>> GetDriverById([FromRoute] int id)
        {
            return Ok(await _driverService.GetAsync(id));
        }

        [HttpGet("getDriverTripsPagedList")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<ActionResult<GetPagedListResponseModel<GetDriverTripResponseModel>>> GetDriverTripsPagedList(
            [FromQuery] GetDriverTripsRequestModel driverTripsRequest,
            [FromQuery] GetPagedListRequestModel<DriverTripSortField> pagedListRequest)
        {
            return Ok(await _driverTripService.GetPagedListAsync(driverTripsRequest, pagedListRequest));
        }

        [HttpGet("getDriverTripSummary/{driverId:int}")]
        [Authorize(Roles = UserRoles.AdminOrDispatcher)]
        public async Task<ActionResult<GetDriverTripSummaryResponseModel>> GetDriverTripSummary([FromRoute] int driverId)
        {
            return Ok(await _driverTripService.GetSummaryAsync(driverId));
        }

        [HttpPost("createDriver")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<GetDriverResponseModel>> CreateDriver([FromBody] CreateDriverRequestModel model)
        {
            var driver = await _driverService.CreateAsync(model);
            return CreatedAtAction(nameof(GetDriverById), new { id = driver.Id }, driver);
        }

        [HttpPut("updateDriver")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UpdateDriver([FromBody] UpdateDriverRequestModel model)
        {
            await _driverService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("deleteDriver/{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<DeleteDriverResponseModel>> DeleteDriver([FromRoute] int id)
        {
            return Ok(await _driverService.DeleteAsync(id));
        }

        [HttpPost("restoreDriver/{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> RestoreDriver([FromRoute] int id)
        {
            await _driverService.RestoreAsync(id);
            return NoContent();
        }
    }
}
