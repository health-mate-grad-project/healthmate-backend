using healthmate_backend.DTOs;
using healthmate_backend.Models;
using Microsoft.EntityFrameworkCore;
using healthmate_backend.Models.Request;



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

            if (appointment.Status != "Pending")
            {
                return false;
            }

            appointment.Status = "Scheduled";

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
                    .Include(a => a.Doctor) // Include Doctor details
                    .Include(a => a.Patient) // Include Patient details
                    .Select(a => new AppointmentDTO
                    {
                        AppointmentId = a.Id,
                        AppointmentType = a.AppointmentType,
                        Date = a.Date,
                        Status = a.Status,
                        Time = a.Time,
                        Content = a.Content,
                        DoctorName = a.Doctor.Username,
                        PatientName = a.Patient.Username,
                        DoctorId = a.DoctorId,
                        PatientId = a.PatientId,
                        Speciality = a.Doctor.Speciality // Include Doctor's speciality
                    })
                    .ToListAsync();
            }
            else if (userRole.ToLower() == "doctor")
            {
                appointments = await _context.Appointments
                    .Where(a => a.DoctorId == userId)
                    .Include(a => a.Doctor) // Include Doctor details
                    .Include(a => a.Patient) // Include Patient details
                    .Select(a => new AppointmentDTO
                    {
                        AppointmentId = a.Id,
                        AppointmentType = a.AppointmentType,
                        Date = a.Date,
                        Status = a.Status,
                        Time = a.Time,
                        Content = a.Content,
                        DoctorName = a.Doctor.Username,
                        PatientName = a.Patient.Username,
                        DoctorId = a.DoctorId,
                        PatientId = a.PatientId,
                        Speciality = a.Doctor.Speciality // Include Doctor's speciality
                    })
                    .ToListAsync();
            }

            return appointments;
        }
        public async Task<bool> CancelAppointmentAsync(int appointmentId, int patientId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);

            if (appointment == null)
                return false;

            if (appointment.Status != "Pending" && appointment.Status != "Scheduled")
                return false;

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> RescheduleAppointmentAsync(int patientId, RescheduleAppointmentRequest req)
        {
            var appt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == req.AppointmentId && a.PatientId == patientId);

            if (appt == null ||
                (appt.Status != "Pending" && appt.Status != "Scheduled"))
                return false;

            var slot = await _context.AvailableSlots
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.Id == req.NewSlotId);

            if (slot == null || slot.IsBooked || slot.DoctorId != appt.DoctorId)
                return false;

            var oldSlot = await _context.AvailableSlots
                .FirstOrDefaultAsync(s => s.Id == appt.AvailableSlotId);
            if (oldSlot != null) oldSlot.IsBooked = false;

            appt.Date = slot.Date.Date;
            appt.Time = slot.StartTime;
            appt.AvailableSlotId = slot.Id;
            appt.Status = "Rescheduled";

            slot.IsBooked = true;

            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> BookAppointmentAsync(int patientId, BookingAppointmentRequest req)
        {
            // Check slot availability and doctor
            var slot = await _context.AvailableSlots
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.Id == req.AvailableSlotId && s.DoctorId == req.DoctorId);

            if (slot == null || slot.IsBooked)
            {
                // _logger.LogWarning("Slot invalid or already booked. SlotId: {SlotId}, IsBooked: {IsBooked}", req.AvailableSlotId, slot?.IsBooked);

                return false;
            }

            // Check if patient already has an appointment at the same date/time (and not cancelled or past)
            var hasConflict = await _context.Appointments
                .AnyAsync(a => a.PatientId == patientId &&
                               a.Date == slot.Date.Date &&
                               a.Time == slot.StartTime &&
                               a.Status != "Cancelled" && a.Status != "Past");

            if (hasConflict)
                return false;

            var patient = await _context.Patients.FindAsync(patientId);
            var doctor = await _context.Doctors.FindAsync(req.DoctorId);
            
            if (patient == null || doctor == null)
                return false;
            
            // Create the appointment entity with your model's required fields
            var appointment = new Appointment
            {
                PatientId = patientId,
                Patient = patient,  
                DoctorId = req.DoctorId,
                Doctor = doctor,    
                AppointmentType = req.AppointmentType,
                Content = req.Content,
                Date = slot.Date.Date,
                Time = slot.StartTime,
                AvailableSlotId = slot.Id,
                Status = "Scheduled", 
                IsRated = false
            };

            // Mark the slot as booked
            slot.IsBooked = true;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return true;
        }

        
    }
    
   
}
