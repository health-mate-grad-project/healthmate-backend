using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;
using healthmate_backend.Models.Request;

namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorHomeScreenController : ControllerBase
    {
        private readonly DoctorHomeScreenService _doctorHomeScreenService;

        public DoctorHomeScreenController(DoctorHomeScreenService doctorHomeScreenService)
        {
            _doctorHomeScreenService = doctorHomeScreenService;
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("upcoming-appointments")]
        public async Task<IActionResult> GetUpcomingAppointments()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var doctorId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var appointments = await _doctorHomeScreenService.GetUpcomingAppointmentsAsync(doctorId);
            return Ok(appointments);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("appointment-patient-details/{appointmentId}")]
        public async Task<IActionResult> GetAppointmentPatientDetails(int appointmentId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var doctorId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var patientDetails = await _doctorHomeScreenService.GetAppointmentPatientDetailsAsync(appointmentId, doctorId);
            if (patientDetails == null)
                return NotFound(new { message = "Appointment not found or unauthorized access" });

            return Ok(patientDetails);
        }
    }
}