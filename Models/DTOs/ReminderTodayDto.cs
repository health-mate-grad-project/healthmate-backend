namespace healthmate_backend.Models.DTOs;

public record ReminderDoseDto(
    int DoseId,
    DateTime ScheduledUtc,
    bool Taken
);
public record ReminderTodayDto(
    int    ReminderId,
    string MedicationName,
    string Dosage,
    string Notes,
    List<ReminderDoseDto> Schedule
    );