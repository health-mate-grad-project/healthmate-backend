using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using healthmate_backend.Services;
using healthmate_backend.Models;
using healthmate_backend.Models.Request;
using healthmate_backend.Models.DTOs;

namespace healthmate_backend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PatientService _patientService;
        private readonly DoctorService _doctorService;
		private readonly GeoLocationService _geoLocationService;

        public PatientController(PatientService patientService, AppDbContext context,DoctorService doctorService,GeoLocationService geoLocationService)
        {
            _patientService = patientService;
            _context = context;
            _doctorService = doctorService;
			_geoLocationService = geoLocationService;

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

[HttpGet("{patientId}/fcm-token")]
public async Task<IActionResult> GetFcmTokenByPatientId(int patientId, [FromHeader(Name = "X-API-KEY")] string apiKey)
{
    if (apiKey != "super-secret-key")
        return Unauthorized(new { message = "Invalid API key" });

    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == patientId);
    if (patient == null)
        return NotFound(new { message = "Patient not found" });

    return Ok(new
    {
        patientId = patient.Id,
        fcmToken = patient.FcmToken
    });
}

[Authorize(Roles = "patient")]
[HttpPut("update-fcm-token")]
public async Task<IActionResult> UpdateFcmToken([FromBody] FcmTokenUpdateRequest request)
{
    var userId = int.Parse(User.FindFirst("UserId").Value);

    var patient = await _context.Patients.FindAsync(userId);
    if (patient == null)
        return NotFound(new { message = "Patient not found" });

    patient.FcmToken = request.FcmToken;
    await _context.SaveChangesAsync();

    return Ok(new { message = "FCM token updated" });
}

public class FcmTokenUpdateRequest
{
    public string FcmToken { get; set; }
}

        // Search doctors based on doctor name or specialty (not both required)
        [Authorize(Roles = "patient")]
[HttpPost("search-doctors")]
public async Task<IActionResult> SearchDoctors([FromBody] DoctorSearchRequest request)
{
    // Check if at least one field is provided
    if (string.IsNullOrEmpty(request.DoctorName) && string.IsNullOrEmpty(request.Speciality))
        return BadRequest(new { message = "Please provide either a doctor's name or a speciality." });

    // Perform search based on provided fields
    var doctorsQuery = _context.Doctors
        .Join(
            _context.Users, 
            doctor => doctor.Id, // Assuming Doctor has a UserId referring to Users table
            user => user.Id,
            (doctor, user) => new { Doctor = doctor, User = user }
        )
        .Where(d =>
            (request.DoctorName != null && d.User.Username.Contains(request.DoctorName)) || // Search by name
            (request.Speciality != null && d.Doctor.Speciality.Contains(request.Speciality))) // Search by speciality
        .Select(d => new DoctorSearchResponse
        {
            DoctorId = d.Doctor.Id,                             // ✅ Include this line
            DoctorName = d.User.Username, // Doctor's name comes from the Users table
            Speciality = d.Doctor.Speciality,
            AverageRating = d.Doctor.AverageRating,
            TotalRatings = d.Doctor.TotalRatings,
            // Calculate the number of filled stars (max 5 stars)
            FilledStars = (int)Math.Round(d.Doctor.AverageRating),
            ProfileImageUrl = d.Doctor.ProfileImageUrl // Add profile image URL
        });

    var doctors = await doctorsQuery.ToListAsync();

    if (doctors.Count == 0)
        return NotFound(new { message = "No doctors found" });

    return Ok(doctors);
}

        
        [Authorize(Roles = "patient")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token: no UserId" });

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });

            var success = await _patientService.UpdateProfileAsync(userId, request);
            if (!success)
                return NotFound(new { message = "Patient not found" });

            return Ok(new { message = "Patient profile updated successfully" });
        }
        
        
        [HttpGet("profile")]
        public async Task<IActionResult> GetPatientProfile()
        {
            // Assuming the patient is identified by the logged-in user's JWT token (e.g., via Claims)
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: no UserId" });
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });
            }

            var patient = await _context.Patients
                .Where(p => p.Id == userId)
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound(new { message = "Patient not found" });
            }

            // Map the patient model to PatientDTO
            var patientDTO = new PatientDTO
            {
                Id = patient.Id,
                Name = patient.Username, // Assuming 'Username' is the name field
                Email = patient.Email,
                DateOfBirth = patient.Birthdate,
                BloodType = patient.BloodType,
                Height = patient.Height,
                Weight = patient.Weight,
                Location = patient.Location,
                ProfileImageUrl = patient.ProfileImageUrl  // Add profile image URL
            };

            return Ok(patientDTO);
        }
        
        [HttpGet("user-profile")]
        public async Task<IActionResult> GetPUserProfile()
        {
            // Assuming the patient is identified by the logged-in user's JWT token (e.g., via Claims)
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token: no UserId" });
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid token: UserId is not valid" });
            }

            // Fetch user from the Users table using the userId
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Map the user model to UserDTO
            var userDTO = new UserDTO
            {
                Username = user.Username,  // Get the Username from the Users table
                Password = user.Password,   // Get the Password (note: this is usually hashed in a real-world scenario)
                Type = user.Type,   // Get type
                Email = user.Email,  // Get the email
                ProfileImageUrl = user.ProfileImageUrl  // Get the profile image URL
            };

            return Ok(userDTO);
        }
        [Authorize(Roles = "patient")]
        [HttpGet("doctor-details/{doctorId}")]
        public async Task<IActionResult> GetDoctorDetails(int doctorId)
        {
            // Fetch doctor details by doctorId
            var doctorDetails = await _doctorService.GetDoctorDetailsByIdAsync(doctorId);
            if (doctorDetails == null)
                return NotFound(new { message = "Doctor not found" });

            return Ok(doctorDetails);
        }

		[Authorize(Roles = "patient")]
[HttpPost("add-reminder")]
public async Task<IActionResult> AddReminderAsPatient([FromBody] CreateReminderRequest request)
{
    var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
    if (userIdClaim == null)
        return Unauthorized(new { message = "Invalid token: no UserId" });

    if (!int.TryParse(userIdClaim.Value, out var patientId))
        return Unauthorized(new { message = "Invalid token: UserId is not valid" });

    var success = await _patientService.AddReminderAsync(request, patientId);
    if (!success)
        return BadRequest(new { message = "Failed to add reminder" });

    return Ok(new { message = "Reminder added successfully" });
}
[Authorize(Roles = "patient")]
[HttpGet("nearby-doctors")]
public async Task<IActionResult> GetNearbyDoctors([FromQuery] string? city)
{
    // Fallback to IP only if city is not provided
    if (string.IsNullOrWhiteSpace(city))
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrEmpty(ip) || ip == "::1")
        {
            return BadRequest(new { message = "Could not determine IP address for location lookup." });
        }

        Console.WriteLine("IP Address Used: " + ip);
        city = await _geoLocationService.GetCityFromIP(ip);
    }

    if (string.IsNullOrWhiteSpace(city))
        return BadRequest(new { message = "Could not determine location." });

    var doctors = await _context.Doctors
        .Include(d => d.Clinics)
        .Where(d => d.Clinics.Any(c => c.Location.Contains(city)))
        .Select(d => new DoctorSearchResponse
        {
            DoctorId = d.Id, 
            DoctorName = d.Username,
            Speciality = d.Speciality,
            AverageRating = d.AverageRating,
            FilledStars = (int)Math.Round(d.AverageRating),
            ProfileImageUrl = d.ProfileImageUrl, // Add profile image URL
            Clinics = d.Clinics
                .Where(c => c.Location.Contains(city))
                .Select(c => new ClinicDto
                {
                    Name = c.Name,
                    Location = c.Location
                }).ToList()
        }).ToListAsync();

    // Debug logging
    foreach (var doctor in doctors)
    {
        Console.WriteLine($"Doctor {doctor.DoctorName} (ID: {doctor.DoctorId}) - ProfileImageUrl: {doctor.ProfileImageUrl}");
    }

    return Ok(doctors);
}

[Authorize(Roles = "patient")]
[HttpGet("test-doctors-profile-images")]
public async Task<IActionResult> TestDoctorsProfileImages()
{
    // Test query to see what's actually in the database
    var doctorsWithImages = await _context.Doctors
        .Select(d => new { 
            DoctorId = d.Id, 
            DoctorName = d.Username, 
            ProfileImageUrl = d.ProfileImageUrl,
            HasProfileImage = !string.IsNullOrEmpty(d.ProfileImageUrl)
        })
        .ToListAsync();

    Console.WriteLine("=== DOCTORS PROFILE IMAGES TEST ===");
    foreach (var doctor in doctorsWithImages)
    {
        Console.WriteLine($"Doctor {doctor.DoctorName} (ID: {doctor.DoctorId}) - HasProfileImage: {doctor.HasProfileImage} - ProfileImageUrl: {doctor.ProfileImageUrl}");
    }
    Console.WriteLine("=== END TEST ===");

    return Ok(doctorsWithImages);
}




    }
    
    

}



