using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;
using healthmate_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly AppDbContext _context;

        public AuthenticationController(AuthenticationService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
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
                var token = await _authService.LoginAsync(request.Email, request.Password);
                var user = await _authService.GetUserByEmailAsync(request.Email);
                if (user != null)
                {
                    var log = new UserLog
                    {
                        UserId = user.Id,
                        Action = "login",
                        Timestamp = DateTime.UtcNow,
                        Details = $"User {user.Email} logged in."
                    };
                    _context.UserLogs.Add(log);
                    await _context.SaveChangesAsync();
                }
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

        [HttpGet("user-logs")]
        public async Task<IActionResult> GetUserLogs()
        {
            var logs = await _context.UserLogs.OrderByDescending(l => l.Timestamp).ToListAsync();
            return Ok(logs);
        }
    }
}