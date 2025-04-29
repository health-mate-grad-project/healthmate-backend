using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;

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
        public async Task<IActionResult> Register(
            string username,
            string password,
            string role,
            string email,
            bool acceptedTerms)
        {
            try
            {
                var user = await _authService.RegisterAsync(username, password, role, email, acceptedTerms);
                return Ok(new { message = "User registered successfully", user.Id, user.Username, user.Email, Role = role });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Registration failed", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                var token = await _authService.LoginAsync(username, password);
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