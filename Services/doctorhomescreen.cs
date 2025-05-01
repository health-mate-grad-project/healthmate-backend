using healthmate_backend.DTOs;
using healthmate_backend.Models;
using Microsoft.EntityFrameworkCore;


namespace healthmate_backend.Services
{
    public class DoctorHomeScreenService
    {
        private readonly AppDbContext _context;

        public DoctorHomeScreenService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppointmentDTO>> GetUpcomingAppointmentsAsync(int doctorId)
        {
            var currentDate = DateTime.UtcNow.Date;
            
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId &&
                            a.Status == "Scheduled" &&
                            a.Date >= currentDate)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Time)
                .Select(a => new AppointmentDTO
                {
                    Id = a.Id,
                    AppointmentType = a.AppointmentType,
                    Date = a.Date,
                    Status = a.Status,
                    Time = a.Time,
                    Content = a.Content,
                    Patient = new PatientBasicDTO
                    {
                        Id = a.Patient.Id,
                        Username = a.Patient.Username,
                        Email = a.Patient.Email
                    }
                })
                .ToListAsync();

            return appointments;
        }

        public async Task<PatientDetailsDTO> GetAppointmentPatientDetailsAsync(int appointmentId, int doctorId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);

            if (appointment == null)
                return null;

            return new PatientDetailsDTO
            {
                Id = appointment.Patient.Id,
                Name = appointment.Patient.Username,
                Email = appointment.Patient.Email,
                DateOfBirth = appointment.Patient.Birthdate,
                BloodType = appointment.Patient.BloodType,
                Height = appointment.Patient.Height,
                Weight = appointment.Patient.Weight,
                Location = appointment.Patient.Location,
                AppointmentDetails = new AppointmentDetailsDTO
                {
                    Id = appointment.Id,
                    AppointmentType = appointment.AppointmentType,
                    Date = appointment.Date,
                    Time = appointment.Time,
                    Status = appointment.Status,
                    Content = appointment.Content
                }
            };
        }
    }
}