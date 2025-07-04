using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using healthmate_backend.Models;
using BCrypt.Net;

namespace healthmate_backend.Services
{
    public class AuthenticationService
    {
        private readonly AppDbContext _context;
        private readonly string _jwtKey;
        private readonly EmailService _emailService;

        public AuthenticationService(AppDbContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _jwtKey = config["Jwt:Key"] ?? throw new ArgumentNullException(nameof(config), "JWT Key is not configured");
            _emailService = emailService;
        }


        public async Task<User> RegisterAsync(string username, string password, string role, string email, bool acceptedTerms)
        {
            if (!acceptedTerms)
                throw new Exception("You must accept the terms and conditions.");

            if (await _context.Users.AnyAsync(u => u.Username == username))
                throw new Exception("Username already exists.");

            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new Exception("Email already exists.");

            User user = role.ToLower() switch


            {
                "doctor" => new Doctor
                {
                    Username = username,
                    Email = email,
                    Type = role,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    License = "", // These will be set later
                    Speciality = "",
                    Clinics = new List<Clinic>()
                },
                "patient" => new Patient
                {
                    Username = username,
                    Email = email,
                    Type = role,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    BloodType = "", // These will be set later
                    Location = ""
                },
                _ => throw new Exception("Invalid role specified")
            };


            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }


        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                throw new Exception("Invalid email or password");

            return GenerateJwtToken(user);
        }


        public async Task<string> LogoutAsync()

        { return "Logged out successfully";
        }


        private string GenerateJwtToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var key = Encoding.UTF8.GetBytes(_jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Email", user.Email),
                    new Claim("Role", user.Type)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            Console.WriteLine($"SendPasswordResetEmailAsync called for email: {email}");
            
            var user = await GetUserByEmailAsync(email);
            if (user == null)
            {
                Console.WriteLine($"User not found for email: {email}");
                return false;
            }

            Console.WriteLine($"User found: {user.Username} (ID: {user.Id})");

            // Generate a dummy reset token (in production, use a secure token or JWT)
            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            // In a real app, save the token to the DB and set an expiry, or use a JWT
            var resetLink = $"https://your-frontend-url.com/reset-password?token={resetToken}";

            var subject = "Health Mate - Password Reset Request";
            var body = $"Dear {user.Username},\n\nWe received a request to reset your password. Please click the link below to reset your password:\n{resetLink}\n\nIf you did not request a password reset, please ignore this email.\n\nBest regards,\nThe Health Mate Team";

            Console.WriteLine($"Attempting to send password reset email to: {user.Email}");

            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, body);
                Console.WriteLine($"Password reset email sent successfully to: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send password reset email to {user.Email}: {ex.Message}");
                Console.WriteLine($"Exception details: {ex}");
                return false;
            }
        }
    }
}
