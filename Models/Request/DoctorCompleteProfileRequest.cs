namespace healthmate_backend.Models.Request
{
    public class DoctorCompleteProfileRequest
    {
        public required string License { get; set; }
        public required string Speciality { get; set; }
        public required List<String> Clinics { get; set; }
    }
} 