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
  		public bool IsRated { get; set; } = false;
    	public int? Rating { get; set; } // 1 to 5, maybe?+
        // Foreign keys
        public int PatientId { get; set; }
        public required Patient Patient { get; set; }

        public int DoctorId { get; set; }
        public required Doctor Doctor { get; set; }
        
        public int? AvailableSlotId { get; set; }
        public AvailableSlot AvailableSlot { get; set; }

    }
}