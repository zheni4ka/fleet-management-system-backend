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
    public class AutoController : ControllerBase
    {
        private readonly IAutoService _autoService;

        public AutoController(IAutoService autoService)
        {
            _autoService = autoService;
        }

        [HttpGet("getAutosPagedList")]
        public async Task<ActionResult<GetPagedListResponseModel<GetAutoResponseModel>>> GetAutosPagedList(
            [FromQuery] GetAutosBySearchFilterRequestModel searchFilter,
            [FromQuery] GetPagedListRequestModel<AutoSortField> pagedListRequest)
        {
            return Ok(await _autoService.GetPagedListAsync(searchFilter, pagedListRequest));
        }

        [HttpGet("getAutoLookupOptions")]
        public async Task<ActionResult<IReadOnlyList<GetAutoLookupOptionResponseModel>>> GetAutoLookupOptions()
        {
            return Ok(await _autoService.GetLookupOptionsAsync());
        }

        [HttpGet("getAutoById/{id:int}")]
        public async Task<ActionResult<GetAutoResponseModel>> GetAutoById([FromRoute] int id)
        {
            return Ok(await _autoService.GetAsync(id));
        }

        [HttpPost("createAuto")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<GetAutoResponseModel>> CreateAuto([FromBody] CreateAutoRequestModel model)
        {
            var auto = await _autoService.CreateAsync(model);
            return CreatedAtAction(nameof(GetAutoById), new { id = auto.Id }, auto);
        }

        [HttpPut("updateAuto")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UpdateAuto([FromBody] UpdateAutoRequestModel model)
        {
            await _autoService.UpdateAsync(model);
            return NoContent();
        }

        [HttpDelete("deleteAuto/{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<DeleteAutoResponseModel>> DeleteAuto([FromRoute] int id)
        {
            return Ok(await _autoService.DeleteAsync(id));
        }

        [HttpPost("restoreAuto/{id:int}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> RestoreAuto([FromRoute] int id)
        {
            await _autoService.RestoreAsync(id);
            return NoContent();
        }
    }
}
