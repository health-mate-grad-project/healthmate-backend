using healthmate_backend.Models;

public class Doctor : User
{
    public required string License { get; set; }
    public required string Speciality { get; set; }
    public int ExperienceYear { get; set; }
    public required ICollection<Clinic> Clinics { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<Reminder> CreatedReminders { get; set; } = new List<Reminder>();
    public double AverageRating { get; set; } = 0.0;
    public int TotalRatings { get; set; } = 0;
}