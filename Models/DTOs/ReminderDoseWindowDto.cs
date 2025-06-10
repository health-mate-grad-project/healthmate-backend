namespace healthmate_backend.Models.DTOs;


    public record ReminderDoseWindowDto(
        int    ReminderId,
        string MedicationName,
        string Dosage,
        string Notes,
        DateTime ScheduledUtc,
        bool   Taken
    );
