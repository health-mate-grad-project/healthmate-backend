using Microsoft.AspNetCore.Mvc;
using healthmate_backend.Services;

namespace healthmate_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OtpController : ControllerBase
    {
        private readonly OtpService _otpService;

        public OtpController(OtpService otpService)
        {
            _otpService = otpService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            try
            {
                await _otpService.GenerateAndSendOtpAsync(request.Email);
                return Ok(new { message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to send OTP", error = ex.Message });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var isValid = await _otpService.VerifyOtpAsync(request.Email, request.Otp);
                if (isValid)
                {
                    return Ok(new { message = "OTP verified successfully" });
                }
                return BadRequest(new { message = "Invalid or expired OTP" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to verify OTP", error = ex.Message });
            }
        }
    }

    public class SendOtpRequest
    {
        public string Email { get; set; }
    }

    public class VerifyOtpRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
} 