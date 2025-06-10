using healthmate_backend.DTOs;
using healthmate_backend.Models;
using healthmate_backend.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace healthmate_backend.Services
{
    public class ReminderService
    {
        private readonly AppDbContext _db;
        public ReminderService(AppDbContext db) => _db = db;

        /* ────────────────────────────────────────────────────────────
           PUBLIC API
        ──────────────────────────────────────────────────────────── */
        public async Task<List<ReminderTodayDto>> GetTodayAsync(
            int patientId, DateTime dayUtc)
        {
            var midnight = dayUtc;
            var end      = midnight.AddDays(1);

            var reminders = await _db.Reminders
                .Where(r => r.PatientId == patientId && r.Repeat)
                .ToListAsync();

            var result = new List<ReminderTodayDto>();
            foreach (var r in reminders)
            {
                var schedule = BuildScheduleForDay(r, midnight, end);

                var takenKeys = await _db.DoseTakens
                    .Where(d => d.ReminderId == r.Id &&
                                d.ScheduledTimeUtc >= midnight &&
                                d.ScheduledTimeUtc <  end)
                    .Select(d => d.ScheduledTimeUtc)
                    .ToListAsync();

                var dtoSchedule = schedule
                    .Select(t => new ReminderDoseDto(
                        t, takenKeys.Contains(t)))
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
            var belongs = await _db.Reminders
                .AnyAsync(r => r.Id == reminderId && r.PatientId == patientId);
            if (!belongs) return false;

            var entry = await _db.DoseTakens.FindAsync(reminderId, scheduledUtc);
            if (entry != null) return true; // already marked

            _db.DoseTakens.Add(new DoseTaken
            {
                ReminderId       = reminderId,
                ScheduledTimeUtc = scheduledUtc,
                TakenTimeUtc     = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return true;
        }

        /* ────────────────────────────────────────────────────────────
           HELPER – per-day schedule
        ──────────────────────────────────────────────────────────── */
        private static List<DateTime> BuildScheduleForDay(
            Reminder r,
            DateTime startUtc,
            DateTime endUtc)
        {
            var list     = new List<DateTime>();
            var interval = GetIntervalMinutes(r.Frequency);
            if (interval <= 0) return list;

            if (!int.TryParse(r.Dosage, out var totalDoses) || totalDoses <= 0)
                return list;

            var first = r.CreatedAt;
            for (var i = 0; i < totalDoses; i++)
            {
                var t = first.AddMinutes(interval * i);
                if (t >= endUtc) break;                 //  ← guard
                if (t >= startUtc) list.Add(t);
            }

            return list;
        }

        /* ────────────────────────────────────────────────────────────
           HELPER – “upcoming” window
        ──────────────────────────────────────────────────────────── */
        public async Task<List<ReminderDoseWindowDto>> GetUpcomingAsync(
            int      patientId,
            DateTime startUtc,
            int      days)
        {
            var windowStart = startUtc.Date;
            var windowEnd   = windowStart.AddDays(days);

            var reminders = await _db.Reminders
                .Where(r => r.PatientId == patientId && r.Repeat)
                .ToListAsync();

            if (reminders.Count == 0)
                return new List<ReminderDoseWindowDto>();

            var reminderIds = reminders.Select(r => r.Id).ToList();
            var takens = await _db.DoseTakens
                .Where(d => reminderIds.Contains(d.ReminderId) &&
                            d.ScheduledTimeUtc >= windowStart &&
                            d.ScheduledTimeUtc <  windowEnd)
                .ToListAsync();

            var takenSet = new HashSet<(int rid, DateTime t)>(
                takens.Select(d => (d.ReminderId, d.ScheduledTimeUtc)));

            var result = new List<ReminderDoseWindowDto>();

            foreach (var r in reminders)
            {
                if (!int.TryParse(r.Dosage, out var totalDoses) || totalDoses <= 0)
                    continue;

                var interval = GetIntervalMinutes(r.Frequency);
                if (interval <= 0) continue;

                for (var i = 0; i < totalDoses; i++)
                {
                    var t = r.CreatedAt.AddMinutes(interval * i);
                    if (t < windowStart || t >= windowEnd) continue;

                    result.Add(new ReminderDoseWindowDto(
                        r.Id,
                        r.MedicationName,
                        r.Dosage,
                        r.Notes ?? string.Empty,
                        t,
                        takenSet.Contains((r.Id, t))
                    ));
                }
            }

            return result.OrderBy(d => d.ScheduledUtc).ToList();
        }

        /* ────────────────────────────────────────────────────────────
           PARSE FREQUENCY → MINUTES
        ──────────────────────────────────────────────────────────── */
        private static int GetIntervalMinutes(string frequency)
        {
            if (string.IsNullOrWhiteSpace(frequency)) return 0;
            frequency = frequency.ToLowerInvariant().Trim();

            // e.g. "every 8 hours", "8h", "8 hr"
            var hourMatch = Regex.Match(frequency, @"(\d+)\s*(hour|hr|h)");
            if (hourMatch.Success)                     // ← FIX
            {
                int n = int.Parse(hourMatch.Groups[1].Value);
                return n * 60;
            }

            // "once daily"
            if (frequency.Contains("once"))
                return 24 * 60;

            // If it's JUST a number → interpret as hours  ← FIX
            if (Regex.IsMatch(frequency, @"^\d+$"))
            {
                int n = int.Parse(frequency);
                return n * 60;
            }

            // legacy: "4 times per day", "4/day"
            var perDay = Regex.Match(frequency, @"(\d+)\s*(x|/)");
            if (perDay.Success)
            {
                int n = int.Parse(perDay.Groups[1].Value);
                return n == 0 ? 0 : (24 * 60 / n);
            }

            return 0; // unsupported format
        }
    }
}
