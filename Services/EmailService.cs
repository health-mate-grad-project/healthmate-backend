using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;

namespace healthmate_backend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly bool _enableSsl;
        private readonly bool _useDefaultCredentials;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            try
            {
                _smtpServer = _configuration["Email:SmtpServer"] ?? throw new ArgumentNullException("Email:SmtpServer");
                _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                _smtpUsername = _configuration["Email:Username"] ?? throw new ArgumentNullException("Email:Username");
                _smtpPassword = _configuration["Email:Password"] ?? throw new ArgumentNullException("Email:Password");
                _fromEmail = _configuration["Email:FromEmail"] ?? throw new ArgumentNullException("Email:FromEmail");
                _enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");
                _useDefaultCredentials = bool.Parse(_configuration["Email:UseDefaultCredentials"] ?? "false");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing EmailService");
                throw;
            }
        }

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                _logger.LogInformation($"Attempting to send OTP email to {toEmail}");

                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = _enableSsl,
                    UseDefaultCredentials = _useDefaultCredentials
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = "Health Mate - Your One-Time Password (OTP)",
                    Body = $"Dear Health Mate User,\n\nYour One-Time Password (OTP) for registration is: {otp}.\n\nThis OTP is valid for the next 5 minutes. Please use it to complete your registration.\n\nIf you did not request this OTP, please ignore this email.\n\nThank you for choosing Health Mate!\n\nBest regards,\nThe Health Mate Team",
                    IsBodyHtml = false
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Successfully sent OTP email to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {toEmail}");
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string body)
        {
            try
            {
                _logger.LogInformation($"Attempting to send password reset email to {toEmail}");
                Console.WriteLine($"EmailService: Attempting to send password reset email to {toEmail}");
                Console.WriteLine($"EmailService: SMTP Server: {_smtpServer}, Port: {_smtpPort}");
                Console.WriteLine($"EmailService: Username: {_smtpUsername}, FromEmail: {_fromEmail}");

                using var client = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = _enableSsl,
                    UseDefaultCredentials = _useDefaultCredentials
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = "Health Mate - Password Reset Request",
                    Body = body,
                    IsBodyHtml = false
                };
                message.To.Add(toEmail);

                Console.WriteLine($"EmailService: Sending email...");
                await client.SendMailAsync(message);
                Console.WriteLine($"EmailService: Email sent successfully to {toEmail}");
                _logger.LogInformation($"Successfully sent password reset email to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EmailService: Failed to send password reset email to {toEmail}");
                Console.WriteLine($"EmailService: Error: {ex.Message}");
                Console.WriteLine($"EmailService: Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, $"Failed to send password reset email to {toEmail}");
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
} 