using Microsoft.AspNetCore.Mvc;
using MiniShop.DTOs;
using MiniShop.Interfaces;
using System.Threading.Tasks;

namespace MiniShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request);

                if (token == null)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                return Ok(new { token });
            }
            catch (System.UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var success = await _authService.RegisterAsync(request);

            if (!success)
            {
                return BadRequest(new { message = "Email already exists" });
            }

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            var token = await _authService.GoogleLoginAsync(request.Token);

            if (token == null)
            {
                return Unauthorized(new { message = "Invalid Google token" });
            }

            return Ok(new { token });
        }
    }
}
