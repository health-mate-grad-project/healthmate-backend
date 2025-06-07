using healthmate_backend.Models;
using healthmate_backend.Models.Request;

using Microsoft.EntityFrameworkCore;

namespace healthmate_backend.Services
{
    public class PatientService
    {
        private readonly AppDbContext _context;

        public PatientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CompleteProfileAsync(int id, PatientCompleteProfileRequest request)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return false;

            patient.Height = request.Height;
            patient.Weight = request.Weight;
            patient.BloodType = request.BloodType;
            patient.Birthdate = request.Birthdate;

            await _context.SaveChangesAsync();
            return true;
        }
		public async Task<List<Doctor>> SearchDoctorsAsync(string query)
		{	
    		return await _context.Doctors
        		.Where(d => d.Username.Contains(query) || d.Speciality.Contains(query))
        		.ToListAsync();
		}

        
        public async Task<bool> UpdateProfileAsync(int id, UpdateProfileRequest request)
        {
            // Fetch the user and patient entities from the database
            var user = await _context.Users.FindAsync(id); // Users table update
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id); // Patients table update

            if (user == null || patient == null)
            {
                // If no user or patient is found, return false
                Console.WriteLine("User or patient not found.");
                return false;
            }

            // Update user information (e.g., name, email) in the Users table
            if (!string.IsNullOrEmpty(request.Name)) 
            {
                user.Username = request.Name;
                Console.WriteLine($"Updated Name: {request.Name}");
            }

            if (!string.IsNullOrEmpty(request.Email)) 
            {
                user.Email = request.Email;
                Console.WriteLine($"Updated Email: {request.Email}");
            }

            // Update patient-specific information (e.g., height, weight, DOB) in the Patients table
            if (request.DateOfBirth.HasValue)
            {
                patient.Birthdate = request.DateOfBirth.Value;
                Console.WriteLine($"Updated DOB: {request.DateOfBirth.Value}");
            }

            if (request.Weight.HasValue)
            {
                patient.Weight = request.Weight.Value;
                Console.WriteLine($"Updated Weight: {request.Weight.Value}");
            }
            if (request.Height.HasValue)
            {
                patient.Height = request.Height.Value;
                Console.WriteLine($"Updated Height: {request.Height.Value}");
            }
            // Save changes to the database
            await _context.SaveChangesAsync();
            return true;
        }
public async Task<bool> AddReminderAsync(CreateReminderRequest request, int patientId)
{
    var patient = await _context.Patients.FindAsync(patientId);
    if (patient == null)
        return false;

    var reminder = new Reminder
    {
        MedicationName = request.MedicationName,
        Dosage = request.Dosage,
        Frequency = request.Frequency,
        Notes = request.Notes,
        Repeat = request.Repeat,
        CreatedAt = DateTime.UtcNow,
        PatientId = patientId,
        Patient = patient,
        CreatedByPatientId = patientId,
        CreatedByPatient = patient
    };

    _context.Reminders.Add(reminder);
    await _context.SaveChangesAsync();
    return true;
}



    }
    
}