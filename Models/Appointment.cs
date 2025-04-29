using System;

namespace healthmate_backend.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public required string AppointmentType { get; set; }
        public DateTime Date { get; set; }
        public required string Status { get; set; }//Upcoming-Past
        public TimeSpan Time { get; set; }
        public required string Content { get; set; }

        // Foreign keys
        public int PatientId { get; set; }
        public required Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public required Doctor Doctor { get; set; }
    }
}