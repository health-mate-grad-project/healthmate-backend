using healthmate_backend.Models.DTOs;

namespace healthmate_backend.Services;
using System.Text.RegularExpressions;
using healthmate_backend.Models;
using healthmate_backend.DTOs;
using Microsoft.EntityFrameworkCore;

public class ReminderService
{
    private readonly AppDbContext _db;
    public ReminderService(AppDbContext db) => _db = db;

    /* -----------------------------------------------------------
       Public API
    ----------------------------------------------------------- */

    public async Task<List<ReminderTodayDto>> GetTodayAsync(int patientId, DateTime todayUtc)
    {
        var midnight = todayUtc.Date;
        var end      = midnight.AddDays(1);

        // bring all active reminders for the patient
        var reminders = await _db.Reminders
            .Where(r => r.PatientId == patientId && r.Repeat == true)
            .ToListAsync();

        var result = new List<ReminderTodayDto>();

        foreach (var r in reminders)
        {
            var schedule = BuildScheduleForDay(r, midnight, end);

            // read DoseTaken rows for those schedule-times
            var takenKeys = await _db.DoseTakens
                .Where(d => d.ReminderId == r.Id &&
                            d.ScheduledTimeUtc >= midnight &&
                            d.ScheduledTimeUtc <  end)
                .Select(d => d.ScheduledTimeUtc)
                .ToListAsync();

            var dtoSchedule = schedule
                .Select(t => new ReminderDoseDto(t,
                                takenKeys.Contains(t)))
                .ToList();

            result.Add(new ReminderTodayDto(
                r.Id,
                r.MedicationName,
                r.Dosage,
                r.Notes ?? string.Empty,
                dtoSchedule));
        }

        return result;
    }

    public async Task<bool> MarkDoseTakenAsync(
        int reminderId, DateTime scheduledUtc, int patientId)
    {
        // quick ownership check
        var belongs = await _db.Reminders
            .AnyAsync(r => r.Id == reminderId && r.PatientId == patientId);
        if (!belongs) return false;

        var entry = await _db.DoseTakens.FindAsync(reminderId, scheduledUtc);
        if (entry != null) return true;               // already marked

        _db.DoseTakens.Add(new DoseTaken
        {
            ReminderId        = reminderId,
            ScheduledTimeUtc  = scheduledUtc,
            TakenTimeUtc      = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return true;
    }

    /* -----------------------------------------------------------
       Helpers
    ----------------------------------------------------------- */
    private static List<DateTime> BuildScheduleForDay(Reminder r, DateTime startUtc, DateTime endUtc)
    {
        var list = new List<DateTime>();
        var interval = GetIntervalMinutes(r.Frequency);
        if (interval <= 0) return list;

        var first = r.CreatedAt;
        while (first < startUtc) first = first.AddMinutes(interval);

        int maxDoses = 0;
        int.TryParse(r.Dosage, out maxDoses); // assuming Dosage is stored as string

        for (var t = first; t < endUtc && list.Count < maxDoses; t = t.AddMinutes(interval))
            list.Add(t);

        return list;
    }

 
    /// Parses "Every 8 hours", "Once daily", "4", etc.
    /// returns interval in minutes (>= 0). 0 => unsupported.
    private static int GetIntervalMinutes(string frequency)
    {
        frequency = frequency.ToLower().Trim();

        if (frequency.Contains("once"))
            return 24 * 60;

        var numMatch = Regex.Match(frequency, @"(\d+)");
        if (!numMatch.Success) return 0;

        var n = int.Parse(numMatch.Groups[1].Value);

        if (frequency.Contains("hour"))
            return n * 60;

        // default = n times per day
        return n == 0 ? 0 : (24 * 60 / n);
    }
}