using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;
using healthmate_backend.Models;
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

        [Authorize(Roles = "doctor")]
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
    }
}
