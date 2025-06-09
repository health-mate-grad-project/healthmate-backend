using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;
using healthmate_backend.Models.Request;
using healthmate_backend.Models.DTOs;
using healthmate_backend.Models;


namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;
        private readonly DoctorService _doctorService;

        public AppointmentController(AppointmentService appointmentService , DoctorService doctorService)
        {
            _appointmentService = appointmentService;
            _doctorService      = doctorService;
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

        [Authorize(Roles = "patient")]
        [HttpPost("cancel-appointment/{appointmentId}")]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var patientId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var success = await _appointmentService.CancelAppointmentAsync(appointmentId, patientId);

            if (!success)
                return BadRequest(new { message = "Appointment must be in Pending or Scheduled status to cancel" });

            return Ok(new { message = "Appointment cancelled successfully" });
        }
        
        [Authorize(Roles = "patient")]
        [HttpPut("reschedule-appointment")]
        public async Task<IActionResult> RescheduleAppointment([FromBody] RescheduleAppointmentRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int patientId))
                return Unauthorized(new { message = "Invalid token: no UserId" });

            var ok = await _appointmentService.RescheduleAppointmentAsync(patientId, request);
            if (!ok)
                return BadRequest(new { message = "Unable to reschedule appointment." });

            return Ok(new { message = "Appointment rescheduled successfully." });
        }
        
   
     
        [Authorize(Roles = "patient")]
        [HttpPost("book")]
        public async Task<IActionResult> BookAppointment([FromBody] BookingAppointmentRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int patientId))
                return Unauthorized(new { message = "Invalid token: no UserId" });

            var result = await _appointmentService.BookAppointmentAsync(patientId, request);

            if (!result)
                return BadRequest(new { message = "Unable to book appointment." });

            return Ok(new { message = "Appointment booked successfully." });
        }
        
        
        
        [Authorize(Roles = "patient")] 
        [HttpGet("get-available-slots-for-doctor/{doctorId}")]
        public async Task<IActionResult> GetAvailableSlotsForDoctor(int doctorId)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            var availableSlots = await _doctorService.GetAvailableSlotsAsync(doctorId);
            if (availableSlots == null || !availableSlots.Any())
                return NotFound(new { message = "No available slots found" });

            var slotDtos = availableSlots.Select(slot => new AvailableSlotDto
            {
                Id = slot.Id,
                Date = slot.Date,
                StartTime = slot.StartTime,
                IsBooked = slot.IsBooked,
                DayOfWeek = slot.DayOfWeek,
                DoctorId = slot.DoctorId,
                Doctor = new DoctorDto
                {
                    Id = doctor.Id,
                    Username = doctor.Username,
                    Email = doctor.Email,
                    License = doctor.License,
                    Speciality = doctor.Speciality,
                    ExperienceYear = doctor.ExperienceYear,
                    AverageRating = doctor.AverageRating,
                    TotalRatings = doctor.TotalRatings,
                    // Location = location 
                }
            }).ToList();

            return Ok(slotDtos);
        }


    }
    
    
    
    
}