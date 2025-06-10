namespace healthmate_backend.Models.Request
{
    public class RescheduleAppointmentRequest
    {
        public int AppointmentId { get; set; }
        public int NewSlotId { get; set; }
    }
}