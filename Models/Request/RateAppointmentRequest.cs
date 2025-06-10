namespace healthmate_backend.Models.Request;

public class RateAppointmentRequest
{
    public required int AppointmentId { get; set; }
    public required int Rating { get; set; }
}