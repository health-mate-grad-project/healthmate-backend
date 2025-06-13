using healthmate_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace healthmate_backend.Services
{
    public class OtpService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private const int OTP_LENGTH = 6;
        private const int OTP_EXPIRY_MINUTES = 5;

        public OtpService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<string> GenerateAndSendOtpAsync(string email)
        {
            // Generate a random 6-digit OTP
            var otp = GenerateOtp();
            var expiryTime = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES);

            // Save OTP to database
            var otpVerification = new OtpVerification
            {
                Email = email,
                Otp = otp,
                ExpiryTime = expiryTime,
                IsVerified = false
            };

            _context.OtpVerifications.Add(otpVerification);
            await _context.SaveChangesAsync();

            // Send OTP via email
            await _emailService.SendOtpEmailAsync(email, otp);

            return otp;
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var verification = await _context.OtpVerifications
                .Where(v => v.Email == email && v.Otp == otp && !v.IsVerified)
                .OrderByDescending(v => v.ExpiryTime)
                .FirstOrDefaultAsync();

            if (verification == null || verification.ExpiryTime < DateTime.UtcNow)
            {
                return false;
            }

            verification.IsVerified = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var otpBytes = new byte[4];
            rng.GetBytes(otpBytes);
            var otp = Math.Abs(BitConverter.ToInt32(otpBytes, 0)) % (int)Math.Pow(10, OTP_LENGTH);
            return otp.ToString().PadLeft(OTP_LENGTH, '0');
        }
    }
} 