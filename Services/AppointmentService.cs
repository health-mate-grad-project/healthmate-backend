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

        public async Task<bool> AcceptAppointmentAsync(int appointmentId, int doctorId)
        {
            // Find the appointment based on appointmentId and doctorId
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);

            if (appointment == null)
                return false;

            // Check if the appointment is in 'Pending' state
            if (appointment.Status != "Pending")
            {
                // If it's not Pending, return false (failure)
                return false;
            }

            // Update the appointment status to 'Scheduled'
            appointment.Status = "Scheduled";

            // Save changes to the database
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> RejectAppointmentAsync(int appointmentId, int doctorId)
        {
            // Find the appointment based on appointmentId and doctorId
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);

            if (appointment == null)
                return false;

            // Check if the appointment is in 'Pending' state
            if (appointment.Status != "Pending")
            {
                // If it's not Pending, return false (failure)
                return false;
            }

            // Update the appointment status to 'Rejected'
            appointment.Status = "Rejected";

            // Save changes to the database
            await _context.SaveChangesAsync();

            return true;
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
