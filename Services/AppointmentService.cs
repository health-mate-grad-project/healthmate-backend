using healthmate_backend.DTOs;
using healthmate_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace healthmate_backend.Services
{
    public class AppointmentService
    {
        private readonly AppDbContext _context;

        public AppointmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppointmentDTO>> GetAppointmentsForUserAsync(int userId, string userRole)
        {
            List<AppointmentDTO> appointments = new List<AppointmentDTO>();

            if (userRole.ToLower() == "patient")
            {
                appointments = await _context.Appointments
                    .Where(a => a.PatientId == userId)
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .Select(a => new AppointmentDTO
                    {
                        AppointmentId = a.Id,
                        AppointmentType = a.AppointmentType,
                        Date = a.Date,
                        Status = a.Status,
                        Time = a.Time,
                        Content = a.Content,
                        DoctorName = a.Doctor.Username,
                        PatientName = a.Patient.Username
                    })
                    .ToListAsync();
            }
            else if (userRole.ToLower() == "doctor")
            {
                appointments = await _context.Appointments
                    .Where(a => a.DoctorId == userId)
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .Select(a => new AppointmentDTO
                    {
                        AppointmentId = a.Id,
                        AppointmentType = a.AppointmentType,
                        Date = a.Date,
                        Status = a.Status,
                        Time = a.Time,
                        Content = a.Content,
                        DoctorName = a.Doctor.Username,
                        PatientName = a.Patient.Username
                    })
                    .ToListAsync();
            }

            return appointments;
        }
    }
}
