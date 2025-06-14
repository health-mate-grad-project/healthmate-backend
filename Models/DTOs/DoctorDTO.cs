namespace healthmate_backend.Models
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string License { get; set; }
        public string Speciality { get; set; }
        public int ExperienceYear { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
    	public string Location { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

}