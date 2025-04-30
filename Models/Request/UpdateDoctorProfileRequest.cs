namespace healthmate_backend.Models.Request;

public class UpdateDoctorProfileRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Specialization { get; set; }
    public DateTime? Birthdate { get; set; }
    public string? License { get; set; }
    public int? ExperienceYear { get; set; }
}