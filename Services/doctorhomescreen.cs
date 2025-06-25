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
                .Include(a => a.Doctor)
                .Where(a => a.DoctorId == doctorId && a.Status == "Scheduled")
                .ToListAsync();

            var upcomingAppointments = new List<AppointmentDTO>();
            foreach (var appointment in appointments)
            {
                if (appointment.Date < currentDate)
                {
                    if (appointment.Status != "Cancelled")
                    {
                        appointment.Status = "Cancelled";
                    }
                }
                else
                {
                    upcomingAppointments.Add(new AppointmentDTO
                    {
                        Id = appointment.Id,
                        AppointmentType = appointment.AppointmentType,
                        Date = appointment.Date,
                        Status = appointment.Status,
                        Time = appointment.Time,
                        Content = appointment.Content,
                        Patient = new PatientBasicDTO
                        {
                            Id = appointment.Patient.Id,
                            Username = appointment.Patient.Username,
                            Email = appointment.Patient.Email,
                            ProfileImageUrl = appointment.Patient.ProfileImageUrl
                        },
                        DoctorProfileImageUrl = appointment.Doctor.ProfileImageUrl
                    });
                }
            }

            await _context.SaveChangesAsync();

            return upcomingAppointments;
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
                ProfileImageUrl = appointment.Patient.ProfileImageUrl,
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

        public async Task<AppointmentActionResult> CancelAppointmentAsync(int appointmentId, int doctorId)
{
    var appointment = await _context.Appointments
        .FirstOrDefaultAsync(a => a.Id == appointmentId && a.DoctorId == doctorId);

    if (appointment == null)
        return new AppointmentActionResult { Success = false, Message = "Appointment not found or unauthorized access" };

    if (appointment.Status == "Cancelled")
        return new AppointmentActionResult { Success = false, Message = "Appointment is already cancelled" };

    if (appointment.Date.Date < DateTime.UtcNow.Date)
        return new AppointmentActionResult { Success = false, Message = "Cannot cancel past appointments" };

    // Mark the appointment as cancelled
    appointment.Status = "Cancelled";

    // Also mark the slot as available
    var slot = await _context.AvailableSlots.FirstOrDefaultAsync(s => s.Id == appointment.AvailableSlotId);
    if (slot != null)
    {
        slot.IsBooked = false;
    }

    await _context.SaveChangesAsync();

    return new AppointmentActionResult { Success = true, Message = "Appointment cancelled successfully" };
}
}

    public class AppointmentActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}