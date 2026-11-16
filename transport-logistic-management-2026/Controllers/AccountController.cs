using business_logic.Constants;
using business_logic.DTOs;
using business_logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using transport_logistic_management_2026.Constants;

namespace transport_logistic_management_2026.Controllers
{
    [ApiController]
    [ApiVersion(ApiVersions.Version1)]
    [Route(ApiVersions.VersionedControllerRoute)]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IExternalLoginService _externalLoginService;

        public AccountController(IAccountService accountService, IExternalLoginService externalLoginService)
        {
            _accountService = accountService;
            _externalLoginService = externalLoginService;
        }

        [HttpPost("loginWithExternalIdentity")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitingPolicyNames.CredentialEndpointsPerIpAddress)]
        public async Task<ActionResult<LoginWithExternalIdentityResponseModel>> LoginWithExternalIdentity([FromBody] LoginWithExternalIdentityRequestModel model)
        {
            return Ok(await _externalLoginService.LoginWithExternalIdentityAsync(model));
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

        [HttpPut("updatePreferredLanguage")]
        [Authorize]
        public async Task<IActionResult> UpdatePreferredLanguage([FromBody] UpdatePreferredLanguageRequestModel model)
        {
            await _accountService.UpdatePreferredLanguageAsync(model);
            return NoContent();
        }

        [HttpPut("updateUserDepots")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> UpdateUserDepots([FromBody] UpdateUserDepotsRequestModel model)
        {
            await _accountService.UpdateUserDepotsAsync(model);
            return NoContent();
        }

        [HttpPost("deactivateUser/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeactivateUser([FromRoute] string id)
        {
            await _accountService.DeactivateUserAsync(id);
            return NoContent();
        }

        [HttpPost("reactivateUser/{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ReactivateUser([FromRoute] string id)
        {
            await _accountService.ReactivateUserAsync(id);
            return NoContent();
        }
    }
}
