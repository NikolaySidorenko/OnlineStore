using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnlineStore.Web.API.Auth.Interfaces;
using OnlineStore.Web.API.Auth.Models;
using System.Net;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IAuthManager _authManager;

        public AuthController(ILogger<UsersController> logger, IAuthManager authManager)
        {
            _logger = logger;
            _authManager = authManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var result = await _authManager.RegisterAsync(model);

            if (result.IsSuccess)
            {
                return StatusCode((int)HttpStatusCode.Created, result);
            }

            return Unauthorized();
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest request)
        {
            if (request.UserName is null || request.Password is null)
            {
                return BadRequest();
            }

            var result = await _authManager.AuthenticateAsync(request.UserName, request.Password);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return Unauthorized();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string token)
        {
            var result = await _authManager.RefreshAsync(token);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return Unauthorized();
        }
    }
}
