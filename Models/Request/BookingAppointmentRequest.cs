namespace healthmate_backend.Models.Request;


public class BookingAppointmentRequest
{
    public int AvailableSlotId { get; set; }
    public int DoctorId { get; set; }
    public string AppointmentType { get; set; } = string.Empty; // required
    public string Content { get; set; } = string.Empty; 
}