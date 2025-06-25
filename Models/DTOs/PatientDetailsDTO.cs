using System;

namespace healthmate_backend.DTOs
{
    public class PatientDetailsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string BloodType { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public string Location { get; set; }
        public string? ProfileImageUrl { get; set; }
        public AppointmentDetailsDTO AppointmentDetails { get; set; }
    }

    public class AppointmentDetailsDTO
    {
        public int Id { get; set; }
        public string AppointmentType { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Status { get; set; }
        public string Content { get; set; }
    }
} 