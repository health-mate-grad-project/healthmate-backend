namespace healthmate_backend.Models;


    public class DoseTaken
    {
        public int  ReminderId     { get; set; }
        public DateTime ScheduledTimeUtc { get; set; }   // PK-part 2
        public DateTime TakenTimeUtc      { get; set; }  // set on insert

        public Reminder Reminder { get; set; } = null!;
    }
