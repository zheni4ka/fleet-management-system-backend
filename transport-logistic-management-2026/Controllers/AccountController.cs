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
        public async Task<ActionResult<ResponseAuth>> Login([FromBody] LoginModel model)
        {
            return Ok(await _accountService.LoginAsync(model));
        }

        [HttpPost("register")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<UserDTO>> Register([FromBody] RegisterModel model)
        {
            var user = await _accountService.RegisterAsync(model);
            return CreatedAtAction(nameof(GetDispatcherById), new { id = user.Id }, user);
        }

        [HttpGet("all")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            return Ok(await _accountService.GetDispatchersAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<UserDTO>> GetDispatcherById([FromRoute] string id)
        {
            return Ok(await _accountService.GetDispatcherByIdAsync(id));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            await _accountService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
