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
        public async Task<List<ReminderTodayDto>> GetTodayAsync(int patientId, DateTime dayUtc)
        {
            var midnight = dayUtc.Date;
            var end = midnight.AddDays(1);

            var reminders = await _db.Reminders
                .Where(r => r.PatientId == patientId && r.Repeat)
                .ToListAsync();

            var result = new List<ReminderTodayDto>();

            foreach (var r in reminders)
            {
                var doses = await _db.Doses
                    .Where(d => d.ReminderId == r.Id &&
                                d.ScheduledUtc >= midnight &&
                                d.ScheduledUtc < end)
                    .ToListAsync();

                var schedule = doses
                    .Select(d => new ReminderDoseDto(d.ScheduledUtc, d.Taken))
                    .OrderBy(d => d.ScheduledUtc)
                    .ToList();

                result.Add(new ReminderTodayDto(
                    r.Id,
                    r.MedicationName,
                    r.Dosage,
                    r.Notes ?? string.Empty,
                    schedule));
            }

            return result;
        }

        public async Task<bool> MarkDoseTakenAsync(int doseId, int patientId)
        {
            var dose = await _db.Doses
                .Include(d => d.Reminder)
                .FirstOrDefaultAsync(d => d.Id == doseId && d.Reminder.PatientId == patientId);

            if (dose == null) return false;

            if (!dose.Taken)
            {
                dose.Taken = true;
                dose.TakenTimeUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

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
            int patientId, DateTime startUtc, int days)
        {
            var windowStart = startUtc.Date;
            var windowEnd = windowStart.AddDays(days);

            var doses = await _db.Doses
                .Include(d => d.Reminder)
                .Where(d => d.Reminder.PatientId == patientId &&
                            d.ScheduledUtc >= windowStart &&
                            d.ScheduledUtc < windowEnd)
                .ToListAsync();

            return doses
                .Select(d => new ReminderDoseWindowDto(
                    d.ReminderId,
                    d.Reminder.MedicationName,
                    d.Reminder.Dosage,
                    d.Reminder.Notes ?? string.Empty,
                    d.ScheduledUtc,
                    d.Taken
                ))
                .OrderBy(d => d.ScheduledUtc)
                .ToList();
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
