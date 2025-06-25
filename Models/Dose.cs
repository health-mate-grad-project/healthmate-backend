namespace healthmate_backend.Models
{
    public class Dose
    {
        public int Id { get; set; }  // Unique dose ID (PK)

        public int ReminderId { get; set; }
        public Reminder Reminder { get; set; } = null!;

        public DateTime ScheduledUtc { get; set; }

        public bool Taken { get; set; } = false;

        public DateTime? TakenTimeUtc { get; set; } // Optional: track when it was taken
    }
}