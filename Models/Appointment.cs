using System;

namespace healthmate_backend.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string AppointmentType { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }//Upcoming-Past
        public TimeSpan Time { get; set; }
        public string Content { get; set; }

        // Foreign keys
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
    }
}