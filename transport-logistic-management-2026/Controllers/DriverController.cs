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
    public class DriverController : ControllerBase
    {
        private readonly IDriverService _driverService;

        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
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
