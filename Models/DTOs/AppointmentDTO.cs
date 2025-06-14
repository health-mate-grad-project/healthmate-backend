using System;

namespace healthmate_backend.Models.DTOs
{
    public class AppointmentDTO
    {
        public int Id { get; set; }
        public string AppointmentType { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public TimeSpan Time { get; set; }
        public string Content { get; set; }
        
        // Include only necessary patient information
        public PatientBasicDTO Patient { get; set; }
        public int AppointmentId { get; set; }
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
        
        // Add DoctorId and PatientId
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        // Add Doctor's speciality
        public string Speciality { get; set; }
        
        // Add rating information
        public bool IsRated { get; set; }
        public int? Rating { get; set; }
        
        // Add doctor's profile image URL
        public string? DoctorProfileImageUrl { get; set; }
    }

    public class PatientBasicDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}