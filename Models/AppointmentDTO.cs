namespace healthmate_backend.Models
{
    public class AppointmentDTO
    {
        public int AppointmentId { get; set; }
        public string AppointmentType { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public TimeSpan Time { get; set; }
        public string Content { get; set; }
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
    }
}