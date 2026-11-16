using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseModel>> Login([FromBody] LoginRequestModel model)
        {
            return Ok(await _accountService.LoginAsync(model));
        }

        [HttpPost("registerAccount")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<GetDispatcherResponseModel>> RegisterAccount([FromBody] RegisterAccountRequestModel model)
        {
            var account = await _accountService.RegisterAsync(model);
            return CreatedAtAction(nameof(GetDispatcherById), new { id = account.Id }, account);
        }

        [HttpGet("getDispatchersPagedList")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<GetPagedListResponseModel<GetDispatcherResponseModel>>> GetDispatchersPagedList(
            [FromQuery] GetPagedListRequestModel<DispatcherSortField> pagedListRequest)
        {
            return Ok(await _accountService.GetDispatchersPagedListAsync(pagedListRequest));
        }

        [HttpGet("getDispatcherById/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<GetDispatcherResponseModel>> GetDispatcherById([FromRoute] string id)
        {
            return Ok(await _accountService.GetDispatcherByIdAsync(id));
        }

        [HttpDelete("deleteUser/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            await _accountService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
