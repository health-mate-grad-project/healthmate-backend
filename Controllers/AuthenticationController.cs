using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Models;
using healthmate_backend.Services;
//comment 
namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthenticationController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                var registeredUser = await _authService.RegisterAsync(user, user.Password);
                return Ok(new { message = "User registered successfully", user = registeredUser });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            try
            {
                var loggedInUser = await _authService.LoginAsync(user.Username, user.Password);
                return Ok(new { message = "Login successful", user = loggedInUser });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
} 