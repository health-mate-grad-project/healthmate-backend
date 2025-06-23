using healthmate_backend.Models;

public class Patient : User
{
    public float Height { get; set; }
    public float Weight { get; set; }
    public required string BloodType { get; set; }
    public DateTime Birthdate { get; set; }
    public required string Location { get; set; } 

    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
public string? FcmToken { get; set; }

}