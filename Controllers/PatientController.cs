using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;
using healthmate_backend.Models;

namespace healthmate_backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly PatientService _patientService;

        public PatientController(PatientService patientService)
        {
            _patientService = patientService;
        }

        [Authorize(Roles = "patient")]
        [HttpPut("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] PatientCompleteProfileRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var success = await _patientService.CompleteProfileAsync(userId, request);
            if (!success)
                return NotFound(new { message = "Patient not found" });

            return Ok(new { message = "Patient profile completed successfully" });
        }
    }
}