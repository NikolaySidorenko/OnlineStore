using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineStore.Data.Providers.Context;
using System;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "IpAddress")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly OnlineStoreContext _context;
        private readonly Random _random;

        public UsersController(ILogger<UsersController> logger, OnlineStoreContext context)
        {
            _logger = logger;
            _context = context;
            _random = new Random();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var waitingTime = _random.Next(1, 11);

            _logger.LogInformation("Some one want to get all users");
            try
            {
                var users = await _context.Users.Include(u => u.Roles).ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception");
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] string user)
        {
            await Task.Delay(1000);

            return Ok(user);
        }
    }
}
