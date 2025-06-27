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
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            int? userId = null;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var parsedId))
                userId = parsedId;
            var result = await _authService.LogoutAsync();
            if (userId != null)
            {
                var log = new UserLog
                {
                    UserId = userId.Value,
                    Action = "logout",
                    Timestamp = DateTime.UtcNow,
                    Details = $"User {userId.Value} logged out."
                };
                _context.UserLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = result });
        }

        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { error = "Email and new password are required." });

            // Check if OTP is verified and not expired
            var otpVerification = await _context.OtpVerifications
                .Where(v => v.Email == request.Email && v.IsVerified && v.ExpiryTime > DateTime.UtcNow)
                .OrderByDescending(v => v.ExpiryTime)
                .FirstOrDefaultAsync();

            if (otpVerification == null)
                return BadRequest(new { error = "OTP not verified or expired." });

            var user = await _authService.GetUserByEmailAsync(request.Email);
            if (user == null)
                return NotFound(new { error = "User not found." });

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successful." });
        }
        

        public class ResetPasswordRequest
        {
            public string Email { get; set; }
            public string NewPassword { get; set; }
        }
    }
}