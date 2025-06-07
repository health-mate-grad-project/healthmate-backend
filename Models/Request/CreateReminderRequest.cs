namespace healthmate_backend.Models.Request
{
    public class CreateReminderRequest
    {
        public int? PatientId { get; set; }
        public string MedicationName { get; set; } = null!;
        public string Dosage { get; set; } = null!;
        public string Frequency { get; set; } = null!;
        public string Notes { get; set; } = null!;
        public bool Repeat { get; set; } = true;
    }
}