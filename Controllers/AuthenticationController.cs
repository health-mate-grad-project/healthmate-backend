using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;
using healthmate_backend.Models;

namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authService;

        public AuthenticationController(AuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.RegisterAsync(request.Username, request.Password, request.Role, request.Email, request.AcceptedTerms);
                return Ok(new { message = "User registered successfully", user.Id, user.Username, user.Email, Role = request.Role });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Registration failed", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request.Username, request.Password, request.Role);
                return Ok(new { message = "Login successful", token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = "Login failed", error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogoutAsync();
            return Ok(new { message = result });
        }
    }
}