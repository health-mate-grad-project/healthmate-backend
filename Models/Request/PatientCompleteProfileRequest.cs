namespace healthmate_backend.Models
{
    public class PatientCompleteProfileRequest
    {
        public float Height { get; set; }
        public float Weight { get; set; }
        public string BloodType { get; set; }
        public DateTime Birthdate { get; set; }
    }
}