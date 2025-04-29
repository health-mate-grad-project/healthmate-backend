using healthmate_backend.Models;

public class Doctor : User
{
    public string License { get; set; }
    public string Speciality { get; set; }
    public int ExperienceYear { get; set; }
    public ICollection<Clinic> Clinics { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<Reminder> CreatedReminders { get; set; } = new List<Reminder>();

}
