namespace healthmate_backend.Models.DTOs;

public class AvailableSlotDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public bool IsBooked { get; set; }
    public string DayOfWeek { get; set; }
    public int DoctorId { get; set; }

    // Optionally, include DoctorDto if you want to include more doctor details
    public DoctorDto Doctor { get; set; }
}