using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;
using healthmate_backend.Models.Request;
using healthmate_backend.Models.DTOs;
using healthmate_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService _appointmentService;
        private readonly DoctorService _doctorService;
        private readonly AppDbContext _context;

        public AppointmentController(AppointmentService appointmentService, DoctorService doctorService, AppDbContext context)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _context = context;
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
        [HttpGet("past-appointments")]
        public async Task<IActionResult> GetPastAppointments()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var patientId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var appointments = await _appointmentService.GetPastAppointmentsForPatientAsync(patientId);

            if (appointments == null || !appointments.Any())
            {
                return NotFound(new { message = "No past appointments found" });
            }

            return Ok(appointments);
        }

        [Authorize(Roles = "patient")]
        [HttpPost("rate-appointment")]
        public async Task<IActionResult> RateAppointment([FromBody] RateAppointmentRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var patientId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            // Validate rating
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5" });

            var success = await _appointmentService.RateAppointmentAsync(request.AppointmentId, patientId, request.Rating);

            if (!success)
                return BadRequest(new { message = "Unable to rate appointment. Make sure it's a past appointment and not already rated." });

            return Ok(new { message = "Appointment rated successfully" });
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
            // Log cancel
            var log = new UserLog
            {
                UserId = patientId,
                Action = "cancel",
                Timestamp = DateTime.UtcNow,
                Details = $"User {patientId} canceled appointment {appointmentId}."
            };
            _context.UserLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Appointment cancelled successfully" });
        }
        
        [Authorize(Roles = "patient")]
        [HttpPut("reschedule-appointment")]
        public async Task<IActionResult> RescheduleAppointment([FromBody] RescheduleAppointmentRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int patientId))
                return Unauthorized(new { message = "Invalid token: no UserId" });

            var (ok, reason) = await _appointmentService.RescheduleAppointmentAsync(patientId, request);

            if (!ok)
                return BadRequest(new { message = reason });
            // Log reschedule
            var log = new UserLog
            {
                UserId = patientId,
                Action = "reschedule",
                Timestamp = DateTime.UtcNow,
                Details = $"User {patientId} rescheduled an appointment."
            };
            _context.UserLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok(new { message = reason });
        }

        [Authorize(Roles = "patient")]
        [HttpPost("book")]
        public async Task<IActionResult> BookAppointment([FromBody] BookingAppointmentRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int patientId))
                return Unauthorized(new { message = "Invalid token: no UserId" });

            var (success, reason) = await _appointmentService.BookAppointmentAsync(patientId, request);

            if (!success)
                return BadRequest(new { message = reason });
            // Log booking
            var log = new UserLog
            {
                UserId = patientId,
                Action = "book",
                Timestamp = DateTime.UtcNow,
                Details = $"User {patientId} booked an appointment."
            };
            _context.UserLogs.Add(log);
            await _context.SaveChangesAsync();
            return Ok(new { message = reason });
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
                    ProfileImageUrl = doctor.ProfileImageUrl
                }
            }).ToList();

            return Ok(slotDtos);
        }

        [Authorize(Roles = "patient")]
        [HttpGet("upcoming-appointments")]
        public async Task<IActionResult> GetUpcomingAppointments()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var patientId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var appointments = await _appointmentService.GetUpcomingAppointmentsForPatientAsync(patientId);

            if (appointments == null || !appointments.Any())
            {
                return NotFound(new { message = "No upcoming appointments found" });
            }

            return Ok(appointments);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPut("start-appointment/{appointmentId}")]
        public async Task<IActionResult> StartAppointment(int appointmentId)
        {
            var doctorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (doctorIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(doctorIdClaim.Value, out var doctorId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            // Here you would call your service to start the appointment (update status, etc.)
            // For now, just log the action
            var log = new UserLog
            {
                UserId = doctorId,
                Action = "start",
                Timestamp = DateTime.UtcNow,
                Details = $"Doctor {doctorId} started appointment {appointmentId}."
            };
            _context.UserLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Appointment {appointmentId} started." });
        }

    }
    
    
    
    
}