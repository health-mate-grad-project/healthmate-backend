using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;
using healthmate_backend.Models.Request;

namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly DoctorService _doctorService;

        public DoctorController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }
[Authorize(Roles = "Doctor")]
[HttpGet("pending-appointments")]
public async Task<IActionResult> GetPendingAppointments()
{
    var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
    if (userIdClaim == null)
        return Unauthorized(new { message = "Invalid token: no UserId" });

    if (!int.TryParse(userIdClaim.Value, out var userId))
        return Unauthorized(new { message = "Invalid token: UserId is not valid" });

    var pendingAppointments = await _doctorService.GetPendingAppointmentsAsync(userId);

    if (pendingAppointments == null || !pendingAppointments.Any())
        return NotFound(new { message = "No pending appointments found." });

    return Ok(pendingAppointments);  // Return the list of simplified AppointmentDTOs
}


        [Authorize(Roles = "Doctor")]
        [HttpPut("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] DoctorCompleteProfileRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var success = await _doctorService.CompleteProfileAsync(userId, request);
            if (!success)
                return NotFound(new { message = "Doctor not found" });

            return Ok(new { message = "Doctor profile completed successfully" });
        }
        
        [Authorize(Roles = "Doctor")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateDoctorProfileRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var success = await _doctorService.UpdateProfileAsync(userId, request);
            if (!success)
                return NotFound(new { message = "Doctor not found" });

            return Ok(new { message = "Doctor profile updated successfully" });
        }
[Authorize(Roles = "Doctor")]
[HttpPost("search-patients")]
public async Task<IActionResult> SearchPatientsWithAppointments([FromBody] PatientSearchRequest request)
{
     var doctorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
    if (doctorIdClaim == null)
        return Unauthorized("Doctor ID not found in token");

    var doctorId = int.Parse(doctorIdClaim.Value);
    var patientName = request.PatientName;

    var patients = await _doctorService.GetPatientsByDoctorAndNameAsync(doctorId, patientName);

    if (patients == null || !patients.Any())
        return NotFound("No matching patients found for this doctor.");

    return Ok(patients);
}
[Authorize(Roles = "Doctor")]
        [HttpGet("doctor-details")]
        public async Task<IActionResult> GetDoctorDetails()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            // Fetch doctor details (using full DoctorDto but using only the necessary fields)
            var doctorDetails = await _doctorService.GetDoctorDetailsByIdAsync(userId);
            if (doctorDetails == null)
                return NotFound(new { message = "Doctor not found" });

            return Ok(doctorDetails); // Return the full doctor details DTO (with all properties available)
        }

        
    }
}
