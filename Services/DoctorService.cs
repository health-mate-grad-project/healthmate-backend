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

        public async Task<bool> CompleteProfileAsync(int id, DoctorCompleteProfileRequest request)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;

            doctor.License = request.License;
            doctor.Speciality = request.Speciality;
            doctor.ExperienceYear = request.ExperienceYear;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
