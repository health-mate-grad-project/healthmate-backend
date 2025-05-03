using System;

namespace healthmate_backend.Models
{
    
        public class AvailableSlot
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan StartTime { get; set; }
            public bool IsBooked { get; set; } = false;
            public string DayOfWeek { get; set; } // 'Mon', 'Tue', 'Wed', etc.
            
            public int DoctorId { get; set; }
            public required Doctor Doctor { get; set; }
        }
    

}