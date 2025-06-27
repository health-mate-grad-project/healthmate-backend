using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using healthmate_backend.Models;
using healthmate_backend.Models.Request;

namespace healthmate_backend.Services
{
    public class AdminService
    {
        private readonly AppDbContext _context;
        public AdminService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<Admin?> AuthenticateAsync(string email, string password)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.Email == email && a.Password == password);
        }

        public async Task<List<Clinic>> GetAllClinicsAsync()
        {
            return await _context.Clinics.ToListAsync();
        }

        public async Task<bool> UpdateClinicLocationAsync(int clinicId, string location)
        {
            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId);
            if (clinic == null)
                return false;
            clinic.Location = location;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task LogAdminActionAsync(int adminId, string action, string details)
        {
            var log = new AdminLog
            {
                AdminId = adminId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                Details = details
            };
            _context.AdminLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
} 