using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;
using healthmate_backend.Models.Request;
using healthmate_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            var admin = await _adminService.AuthenticateAsync(request.Email, request.Password);
            if (admin == null)
                return Unauthorized(new { message = "Invalid email or password" });
            await _adminService.LogAdminActionAsync(admin.Id, "login", $"Admin {admin.Email} logged in.");
            return Ok(new { id = admin.Id, name = admin.Name, email = admin.Email });
        }

        [HttpGet("clinics")]
        public async Task<IActionResult> GetAllClinics()
        {
            var clinics = await _adminService.GetAllClinicsAsync();
            return Ok(clinics);
        }

        [HttpPut("clinic/{clinicId}/location")]
        public async Task<IActionResult> UpdateClinicLocation(int clinicId, [FromBody] UpdateClinicLocationRequest request)
        {
            var success = await _adminService.UpdateClinicLocationAsync(clinicId, request.Location);
            if (!success)
                return NotFound(new { message = "Clinic not found" });
            return Ok(new { message = "Clinic location updated successfully" });
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetAdminLogs()
        {
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var context = (AppDbContext)scope.ServiceProvider.GetService(typeof(AppDbContext));
                var logs = await context.AdminLogs.OrderByDescending(l => l.Timestamp).ToListAsync();
                return Ok(logs);
            }
        }

        [HttpGet("user-logs-login")]
        public async Task<IActionResult> GetAllUserLoginLogs([FromServices] AppDbContext context)
        {
            var logs = await context.UserLogs
                .Where(l => l.Action == "login")
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return Ok(new { message = "User login logs", logs });
        }

        [HttpGet("user-logs-logout")]
        public async Task<IActionResult> GetAllUserLogoutLogs([FromServices] AppDbContext context)
        {
            var logs = await context.UserLogs
                .Where(l => l.Action == "logout")
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return Ok(new { message = "User logout logs", logs });
        }

        [HttpGet("user-logs-book")]
        public async Task<IActionResult> GetAllUserBookLogs([FromServices] AppDbContext context)
        {
            var logs = await context.UserLogs
                .Where(l => l.Action == "book")
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return Ok(new { message = "User book appointment logs", logs });
        }

        [HttpGet("user-logs-reschedule")]
        public async Task<IActionResult> GetAllUserRescheduleLogs([FromServices] AppDbContext context)
        {
            var logs = await context.UserLogs
                .Where(l => l.Action == "reschedule")
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return Ok(new { message = "User reschedule appointment logs", logs });
        }

        [HttpGet("user-logs-cancel")]
        public async Task<IActionResult> GetAllUserCancelLogs([FromServices] AppDbContext context)
        {
            var logs = await context.UserLogs
                .Where(l => l.Action == "cancel")
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return Ok(new { message = "User cancel appointment logs", logs });
        }

        [HttpGet("user-logs-start")]
        public async Task<IActionResult> GetAllUserStartLogs([FromServices] AppDbContext context)
        {
            var logs = await context.UserLogs
                .Where(l => l.Action == "start")
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return Ok(new { message = "User start appointment logs", logs });
        }

        [HttpGet("user-logs-reminder")]
        public async Task<IActionResult> GetAllUserReminderLogs([FromServices] AppDbContext context)
        {
            var logs = await context.UserLogs
                .Where(l => l.Action == "reminder")
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return Ok(new { message = "User reminder logs", logs });
        }
    }

    public class AdminLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UpdateClinicLocationRequest
    {
        public string Location { get; set; }
    }
} 