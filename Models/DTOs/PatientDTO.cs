namespace healthmate_backend.Models.Request;

public class PatientDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string BloodType { get; set; }
    public float Height { get; set; }
    public float Weight { get; set; }
    public string Location { get; set; }
}
