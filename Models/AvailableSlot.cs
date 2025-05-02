using System;

namespace healthmate_backend.Models
{
    
        public class AvailableSlot
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public bool IsBooked { get; set; } = false;

            public int DoctorId { get; set; }
            public required Doctor Doctor { get; set; }
        }
    

}