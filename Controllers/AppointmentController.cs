using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;

namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;

        public AppointmentController(AppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("accept-appointment/{appointmentId}")]
        public async Task<IActionResult> AcceptAppointment(int appointmentId)
        {
            var doctorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (doctorIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(doctorIdClaim.Value, out var doctorId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var success = await _appointmentService.AcceptAppointmentAsync(appointmentId, doctorId);

            if (!success)
                return BadRequest(new { message = "Appointment must be in Pending status to accept" });

            return Ok(new { message = "Appointment accepted successfully" });
        }

        [Authorize(Roles = "Doctor")]
        [HttpPost("reject-appointment/{appointmentId}")]
        public async Task<IActionResult> RejectAppointment(int appointmentId)
        {
            var doctorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (doctorIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(doctorIdClaim.Value, out var doctorId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var success = await _appointmentService.RejectAppointmentAsync(appointmentId, doctorId);

            if (!success)
                return BadRequest(new { message = "Appointment must be in Pending status to reject" });

            return Ok(new { message = "Appointment rejected successfully" });
        }

        [Authorize]
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "Role");

            if (userIdClaim == null || roleClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: no UserId or Role" });
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });
            }

            var userRole = roleClaim.Value;

            var appointments = await _appointmentService.GetAppointmentsForUserAsync(userId, userRole);

            if (appointments == null || !appointments.Any())
            {
                return NotFound(new { message = "No appointments found" });
            }

            return Ok(appointments);
        }
    }
}