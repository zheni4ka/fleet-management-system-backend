using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using transport_logistic_management_2026.Constants;

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
        [EnableRateLimiting(RateLimitingPolicyNames.CredentialEndpointsPerIpAddress)]
        public async Task<ActionResult<LoginResponseModel>> Login([FromBody] LoginRequestModel model)
        {
            return Ok(await _accountService.LoginAsync(model));
        }

        [HttpPost("refreshSession")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingPolicyNames.CredentialEndpointsPerIpAddress)]
        public async Task<ActionResult<RefreshSessionResponseModel>> RefreshSession([FromBody] RefreshSessionRequestModel model)
        {
            return Ok(await _accountService.RefreshSessionAsync(model));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestModel model)
        {
            await _accountService.LogoutAsync(model);
            return NoContent();
        }

        [HttpPost("changePassword")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingPolicyNames.CredentialEndpointsPerIpAddress)]
        public async Task<ActionResult<ChangePasswordResponseModel>> ChangePassword([FromBody] ChangePasswordRequestModel model)
        {
            return Ok(await _accountService.ChangePasswordAsync(model));
        }

        [HttpPost("resetUserPassword")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<ResetUserPasswordResponseModel>> ResetUserPassword([FromBody] ResetUserPasswordRequestModel model)
        {
            return Ok(await _accountService.ResetUserPasswordAsync(model));
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
