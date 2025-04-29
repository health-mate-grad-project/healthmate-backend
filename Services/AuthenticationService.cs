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

        public AuthenticationService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _jwtKey = config["Jwt:Key"];
        }

        
        public async Task<User> RegisterAsync(string username, string password, string role, string email, bool acceptedTerms)
        {
            if (!acceptedTerms)
                throw new Exception("You must accept the terms and conditions.");

            if (await _context.Users.AnyAsync(u => u.Username == username))
                throw new Exception("Username already exists.");

            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new Exception("Email already exists.");

            User user;

            if (role == "Doctor")
            {
                user = new Doctor
                {
                    Username = username,
                    Email = email,
                    Type="Doctor",
                    License = "Pending",        
                    Speciality = "General",       
                    ExperienceYear = 0  
                };
            }
            else if (role == "Patient")
            {
                user = new Patient
                {
                    Username = username,
                    Email = email,
                    Type="Patient",
                    Height = 0,
                    Weight = 0,
                    BloodType = "Unknown",
                    Birthdate = DateTime.UtcNow.AddYears(-20)

                };
            }
            else
            {
                throw new Exception("Invalid role.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(password);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Database Error: " + ex.InnerException?.Message ?? ex.Message);
            }


            return user;
        }

      
        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                throw new Exception("Invalid username or password");

            return GenerateJwtToken(user);
        }

        
        public Task<string> LogoutAsync()
        {
            return Task.FromResult("Logged out.");
        }

     
        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("Username", user.Username),
                    new Claim("Role", user.GetType().Name)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
