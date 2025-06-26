namespace healthmate_backend.Models.Request;

public class PatientSearchResponse
{
    public string PatientName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string? ProfileImageUrl { get; set; }
}