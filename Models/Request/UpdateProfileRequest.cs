namespace healthmate_backend.Models
{
    public class UpdateProfileRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public float? Weight { get; set; }
        public float? Height { get; set; }
    }
}