using healthmate_backend.Models.DTOs;

public class DoctorSearchResponse
{
    public string DoctorName { get; set; }
    public string Speciality { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int FilledStars { get; set; }
    public List<ClinicDto>? Clinics { get; set; } // <-- add this

}