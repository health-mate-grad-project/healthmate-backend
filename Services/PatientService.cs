using healthmate_backend.Models;
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
    }
}