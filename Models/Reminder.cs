using System;

namespace healthmate_backend.Models
{
    public class Reminder
    {
        public int Id { get; set; }

        public required string MedicationName { get; set; }     // "Panadol"
        public required string Dosage { get; set; }             // "13" or "13mg"
        public required string Frequency { get; set; }          // "12 hours"
        public required string Notes { get; set; }              // Optional comments

        public bool Repeat { get; set; } = true;       // default from ERD

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Receiver: Patient
        public int PatientId { get; set; }
        public required Patient Patient { get; set; }

        // Creator: Optional Doctor or Patient
        public int? CreatedByDoctorId { get; set; }
        public Doctor? CreatedByDoctor { get; set; }

        public int? CreatedByPatientId { get; set; }
        public Patient? CreatedByPatient { get; set; }
public int? DoctorId { get; set; }
        public Doctor? Doctor { get; set; } = null!;
        
        public DateTime? LastSentAt { get; set; }

    }
}