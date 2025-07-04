using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using healthmate_backend.Models;
using RabbitMQ.Client;

public class ReminderScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ReminderScheduler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private TimeSpan ParseFrequency(string frequency)
    {
        if (string.IsNullOrWhiteSpace(frequency))
            return TimeSpan.Zero;

        frequency = frequency.Trim().ToLower();

        var numberStr = new string(frequency.Where(char.IsDigit).ToArray());
        if (!int.TryParse(numberStr, out int number))
            return TimeSpan.Zero;

        if (frequency.EndsWith("h"))
            return TimeSpan.FromHours(number);
        else if (frequency.EndsWith("m"))
            return TimeSpan.FromMinutes(number);
        else
            return TimeSpan.FromHours(number); // Default
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;
            var reminders = await _context.Reminders.ToListAsync();
            Console.WriteLine($"[Scheduler] Checking at {now}");

            foreach (var reminder in reminders)
            {
                var frequencyTimeSpan = ParseFrequency(reminder.Frequency);
if (frequencyTimeSpan == TimeSpan.Zero)
    continue;

var createdAt = reminder.CreatedAt;
var endDate = createdAt.AddDays(reminder.Repeat);
if (now < createdAt)
    {
        Console.WriteLine($"[Skip] Reminder {reminder.Id} not started yet");
        continue;
    }
if (now > endDate)
{
    Console.WriteLine($"[Skip] Reminder {reminder.Id} expired after {reminder.Repeat} days.");
    continue;
}

// Calculate how many full intervals passed since CreatedAt
var intervalsPassed = (int)((now - createdAt).TotalMinutes / frequencyTimeSpan.TotalMinutes);
var expectedDueTime = createdAt.AddMinutes(intervalsPassed * frequencyTimeSpan.TotalMinutes);

// Only send if LastSentAt is before expectedDueTime
if (reminder.LastSentAt == null || reminder.LastSentAt < expectedDueTime)
{
    new ReminderPublisher().PublishReminder(reminder);
    reminder.LastSentAt = expectedDueTime; // Keep it aligned
    Console.WriteLine($"[Due] Reminder for {reminder.MedicationName} (Patient {reminder.PatientId}) is due.");
}

            }

            // Cleanup expired reminders
            var expiredReminders = reminders
                .Where(r => r.Repeat > 0 && now > r.CreatedAt.AddDays(r.Repeat))
                .ToList();

            foreach (var expired in expiredReminders)
            {
                var doses = _context.Doses.Where(d => d.ReminderId == expired.Id);
                _context.Doses.RemoveRange(doses);
                _context.Reminders.Remove(expired);
                Console.WriteLine($"[Cleanup] Deleted expired reminder for {expired.MedicationName}");
            }

            await _context.SaveChangesAsync();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
