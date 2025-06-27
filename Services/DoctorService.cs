using healthmate_backend.Models;
using healthmate_backend.Models.Request;
using Microsoft.EntityFrameworkCore;
using healthmate_backend.Models.DTOs;  
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
                    .Include(c => c.Doctors)
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
                }

                if (!existingClinic.Doctors.Contains(doctor))
                {
                    existingClinic.Doctors.Add(doctor);
                }

                clinicEntities.Add(existingClinic);
            }

            doctor.Clinics.Clear();
            foreach (var clinic in clinicEntities)
            {
                doctor.Clinics.Add(clinic);
            }

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
        public async Task<List<PatientSearchResponse>> GetPatientsByDoctorAndNameAsync(int doctorId, string patientName)
{
    return await _context.Appointments
        .Include(a => a.Patient)
        .Where(a => a.DoctorId == doctorId &&
                    a.Patient.Username.Contains(patientName)) // or use .ToLower().Contains() for case-insensitive
        .Select(a => new PatientSearchResponse
        {
            PatientName = a.Patient.Username,
            Status = a.Status,
            Date = a.Date,
            Time = a.Time,
            ProfileImageUrl = a.Patient.ProfileImageUrl // Added for patient profile image
        }) // Remove duplicate patients who had multiple appointments
        .ToListAsync();
}
        public async Task<DoctorDto?> GetDoctorDetailsByIdAsync(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Clinics)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return null;

            // Get the first clinic's location if available
            var location = doctor.Clinics.FirstOrDefault()?.Location ?? "No location available";

            return new DoctorDto
            {
                Id = doctor.Id,
                Username = doctor.Username,
                Email = doctor.Email,
                License = doctor.License,
                Speciality = doctor.Speciality,
                ExperienceYear = doctor.ExperienceYear,
                AverageRating = doctor.AverageRating,
                TotalRatings = doctor.TotalRatings,
                Location = location,  // Assign location from the clinic
                ProfileImageUrl = doctor.ProfileImageUrl,  // Add profile image URL
                Clinics = doctor.Clinics.Select(c => new ClinicDto
                {
                    Name = c.Name,
                    Location = c.Location
                }).ToList()
            };
        }

        public async Task<List<AppointmentDTO>> GetPendingAppointmentsAsync(int doctorId)
        {
            var pendingAppointments = await _context.Appointments
                .Include(a => a.Patient)  // Include patient details
                .Where(a => a.DoctorId == doctorId && a.Status == "Pending")
                .Select(a => new AppointmentDTO
                {
                    Id = a.Id,
                    
                    Date = a.Date,
                   
                    Time = a.Time,
                    
                    Patient = new PatientBasicDTO  // Include patient information in DTO
                    {
                        Id = a.Patient.Id,
                        Username = a.Patient.Username,
                        ProfileImageUrl = a.Patient.ProfileImageUrl
                    },
                    DoctorProfileImageUrl = a.Doctor.ProfileImageUrl // Include Doctor's profile image URL
                })
                .ToListAsync();

            return pendingAppointments;
        }
        
        public async Task<bool> SaveAvailableSlotsAsync(int doctorId, List<SlotData> slots)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
                return false;

            foreach (var slot in slots)
            {
                var availableSlot = new AvailableSlot
                {
                    Date = slot.Date,
                    StartTime = slot.StartTime,
                    //  EndTime = slot.EndTime,
                    DoctorId = doctorId,
                    Doctor = doctor, // Initialize the Doctor property
                    DayOfWeek = slot.Date.ToString("ddd")  // Converts Date to DayOfWeek ('Mon', 'Tue', etc.)

                };

                _context.AvailableSlots.Add(availableSlot);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<Doctor?> GetDoctorByIdAsync(int doctorId)
        {
            return await _context.Doctors.FindAsync(doctorId);
        }

        public async Task<List<AvailableSlot>> GetAvailableSlotsAsync(int doctorId)
        {
            return await _context.AvailableSlots
                .Where(slot => slot.DoctorId == doctorId && !slot.IsBooked)  
                .ToListAsync();
        }

        public async Task<List<AvailableSlot>> GetAllDoctorSlotsAsync(int doctorId)
        {
            return await _context.AvailableSlots
                .Where(slot => slot.DoctorId == doctorId )  
                .ToListAsync();
        }
        public async Task<bool> AddReminderAsync(CreateReminderRequest request, int doctorId)
        {
            if (!request.PatientId.HasValue)
                return false;

            var patient = await _context.Patients.FindAsync(request.PatientId.Value);
            if (patient == null)
                return false;

            var reminder = new Reminder
            {
                MedicationName    = request.MedicationName,
                Dosage            = request.Dosage,
                Frequency         = request.Frequency,
                Notes             = request.Notes,
                Repeat            = request.Repeat,
                CreatedAt         = DateTime.UtcNow.AddHours(-1),
                LastSentAt        = DateTime.UtcNow.AddHours(-1),
                PatientId         = request.PatientId.Value,
                Patient           = patient,
                CreatedByDoctorId = doctorId,
                DoctorId          = doctorId
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            // Parse frequency like "8h" or "30m"
            TimeSpan frequencyInterval;
            var freq = reminder.Frequency?.Trim().ToLower();

            if (string.IsNullOrEmpty(freq))
                return false;

            if (freq.EndsWith("h") && double.TryParse(freq[..^1], out double h))
            {
                frequencyInterval = TimeSpan.FromHours(h);
            }
            else if (freq.EndsWith("m") && double.TryParse(freq[..^1], out double m))
            {
                frequencyInterval = TimeSpan.FromMinutes(m);
            }
            else
            {
                return false; // Invalid frequency format
            }

            // Generate doses between [start, end)
            var doses = new List<Dose>();
            DateTime start = reminder.CreatedAt;
            DateTime end = start.AddDays(reminder.Repeat); // repeat = number of days

            for (DateTime dt = start; dt < end; dt = dt.Add(frequencyInterval))
            {
                doses.Add(new Dose
                {
                    ReminderId   = reminder.Id,
                    ScheduledUtc = dt,
                    Taken        = false
                });
            }

            _context.Doses.AddRange(doses);
            await _context.SaveChangesAsync();

            return true;
        }

public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status)
{
    var appointment = await _context.Appointments.FindAsync(appointmentId);
    if (appointment == null) return false;

    appointment.Status = status;
    await _context.SaveChangesAsync();
    return true;
}
public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
{
    return await _context.Appointments.FindAsync(appointmentId);
}

        
    }
}
