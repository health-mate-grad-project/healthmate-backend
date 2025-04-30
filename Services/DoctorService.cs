using healthmate_backend.Models;
using healthmate_backend.Models.Request;
using Microsoft.EntityFrameworkCore;

namespace healthmate_backend.Services
{
    public class DoctorService
    {
        private readonly AppDbContext _context;

        public DoctorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CompleteProfileAsync(int doctorId, DoctorCompleteProfileRequest request)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Clinics)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return false;

            doctor.License = request.License;
            doctor.Speciality = request.Speciality;

            var clinicEntities = new List<Clinic>();

            foreach (var clinicName in request.Clinics.Distinct())
            {
                var existingClinic = await _context.Clinics
                    .FirstOrDefaultAsync(c => c.Name == clinicName);

                if (existingClinic == null)
                {
                    existingClinic = new Clinic
                    {
                        Name = clinicName,
                        Location = "Unknown", 
                        Doctors = new List<Doctor>() 
                    };

                    _context.Clinics.Add(existingClinic);
                    await _context.SaveChangesAsync();
                }

                clinicEntities.Add(existingClinic);
            }

            doctor.Clinics = clinicEntities;
            await _context.SaveChangesAsync();

            return true;
        }
        
        public async Task<bool> UpdateProfileAsync(int id, UpdateDoctorProfileRequest request)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;

            // Update the fields if they are provided
            if (!string.IsNullOrEmpty(request.Name)) doctor.Username = request.Name;
            if (!string.IsNullOrEmpty(request.Email)) doctor.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Specialization)) doctor.Speciality = request.Specialization;
            if (!string.IsNullOrEmpty(request.License)) doctor.License = request.License;
            if (request.ExperienceYear.HasValue) doctor.ExperienceYear = request.ExperienceYear.Value;

            await _context.SaveChangesAsync();
            return true;
        }
        
        
    }
}
