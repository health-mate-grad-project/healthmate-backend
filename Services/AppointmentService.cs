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
                        Speciality = a.Doctor.Speciality, // Include Doctor's speciality
                        IsRated = a.IsRated,
                        Rating = a.Rating,
                        DoctorProfileImageUrl = a.Doctor.ProfileImageUrl, // Include Doctor's profile image URL
                        Patient = new PatientBasicDTO
                        {
                            Id = a.Patient.Id,
                            Username = a.Patient.Username,
                            Email = a.Patient.Email,
                            ProfileImageUrl = a.Patient.ProfileImageUrl
                        }
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
                        Speciality = a.Doctor.Speciality, // Include Doctor's speciality
                        IsRated = a.IsRated,
                        Rating = a.Rating,
                        DoctorProfileImageUrl = a.Doctor.ProfileImageUrl, // Include Doctor's profile image URL
                        Patient = new PatientBasicDTO
                        {
                            Id = a.Patient.Id,
                            Username = a.Patient.Username,
                            Email = a.Patient.Email,
                            ProfileImageUrl = a.Patient.ProfileImageUrl
                        }
                    })
                    .ToListAsync();
            }

            // Debug logging
            foreach (var appointment in appointments)
            {
                Console.WriteLine($"Appointment {appointment.AppointmentId} - Doctor: {appointment.DoctorName} (ID: {appointment.DoctorId}) - ProfileImageUrl: {appointment.DoctorProfileImageUrl}");
            }

            return appointments ?? new List<AppointmentDTO>();
        }

        public async Task<List<AppointmentDTO>> GetPastAppointmentsForPatientAsync(int patientId)
        {
            var currentDate = DateTime.UtcNow.Date;
            
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patientId && 
                           (a.Date < currentDate || a.Status == "Completed" || a.Status == "Cancelled"))
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
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
                    Speciality = a.Doctor.Speciality,
                    IsRated = a.IsRated,
                    Rating = a.Rating,
                    DoctorProfileImageUrl = a.Doctor.ProfileImageUrl, // Include Doctor's profile image URL
                    Patient = new PatientBasicDTO
                    {
                        Id = a.Patient.Id,
                        Username = a.Patient.Username,
                        Email = a.Patient.Email,
                        ProfileImageUrl = a.Patient.ProfileImageUrl
                    }
                })
                .ToListAsync();

            return appointments ?? new List<AppointmentDTO>();
        }

        public async Task<bool> RateAppointmentAsync(int appointmentId, int patientId, int rating)
        {
            // Validate rating range
            if (rating < 1 || rating > 5)
                return false;

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);

            if (appointment == null)
                return false;

            // Check if appointment is past or completed
            var currentDate = DateTime.UtcNow.Date;
            if (appointment.Date > currentDate && appointment.Status != "Completed")
                return false;

            // Check if already rated
            if (appointment.IsRated)
                return false;

            // Update appointment rating
            appointment.Rating = rating;
            appointment.IsRated = true;

            // Update doctor's average rating
            var doctor = appointment.Doctor;
            var doctorAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id && a.IsRated && a.Rating.HasValue)
                .ToListAsync();

            if (doctorAppointments.Any())
            {
                doctor.AverageRating = doctorAppointments.Average(a => a.Rating!.Value);
                doctor.TotalRatings = doctorAppointments.Count;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, int patientId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patientId);

            if (appointment == null)
                return false;

            if (appointment.Status != "Pending" && appointment.Status != "Scheduled" && appointment.Status != "Rescheduled")
                return false;

            appointment.Status = "Cancelled";

            // Mark slot as available again
            var slot = await _context.AvailableSlots.FirstOrDefaultAsync(s => s.Id == appointment.AvailableSlotId);
            if (slot != null)
            {
                slot.IsBooked = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        
        public async Task<(bool Success, string Reason)> RescheduleAppointmentAsync(int patientId, RescheduleAppointmentRequest req)
        {
            var appt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == req.AppointmentId && a.PatientId == patientId);

            if (appt == null)
                return (false, "Appointment not found.");

            if (appt.Status != "Pending" && appt.Status != "Scheduled")
                return (false, "Only pending or scheduled appointments can be rescheduled.");

            var slot = await _context.AvailableSlots
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.Id == req.NewSlotId);

            if (slot == null)
                return (false, "Selected time slot not found.");

            if (slot.IsBooked)
                return (false, "The selected slot is already booked.");

            if (slot.DoctorId != appt.DoctorId)
                return (false, "Slot does not belong to the same doctor.");

            // Release old slot
            var oldSlot = await _context.AvailableSlots
                .FirstOrDefaultAsync(s => s.Id == appt.AvailableSlotId);
            if (oldSlot != null) oldSlot.IsBooked = false;

            // Update appointment
            appt.Date = slot.Date.Date;
            appt.Time = slot.StartTime;
            appt.AvailableSlotId = slot.Id;
            appt.Status = "Pending";
            slot.IsBooked = true;

            await _context.SaveChangesAsync();
            return (true, "Appointment rescheduled successfully.");
        }

        
        public async Task<(bool success, string reason)> BookAppointmentAsync(int patientId, BookingAppointmentRequest req)
        {
            // Check slot availability and doctor
            var slot = await _context.AvailableSlots
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.Id == req.AvailableSlotId && s.DoctorId == req.DoctorId);

            if (slot == null || slot.IsBooked)
            {
                // _logger.LogWarning("Slot invalid or already booked. SlotId: {SlotId}, IsBooked: {IsBooked}", req.AvailableSlotId, slot?.IsBooked);

                return (false, "Slot is already booked or invalid.");
            }

            // Check if patient already has an appointment at the same date/time (and not cancelled or past)
            var hasConflict = await _context.Appointments
                .AnyAsync(a => a.PatientId == patientId &&
                               a.Date == slot.Date.Date &&
                               a.Time == slot.StartTime &&
                               a.Status != "Cancelled" && a.Status != "Completed");

            if (hasConflict)
                return (false, "You already have an appointment at this time.");

            var patient = await _context.Patients.FindAsync(patientId);
            var doctor = await _context.Doctors.FindAsync(req.DoctorId);
            
            if (patient == null || doctor == null)
                return (false, "Invalid patient or doctor information.");
            
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
                Status = "Pending", 
                IsRated = false
            };

            // Mark the slot as booked
            slot.IsBooked = true;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return (true, "Appointment booked successfully.");
        }

        
    }
    
   
}
