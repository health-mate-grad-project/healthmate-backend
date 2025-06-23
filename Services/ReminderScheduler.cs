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

    // Parse frequency string like "30m", "4h", or just "2" (default to hours)
    private TimeSpan ParseFrequency(string frequency)
    {
        if (string.IsNullOrWhiteSpace(frequency))
            return TimeSpan.Zero;

        frequency = frequency.Trim().ToLower();

        // Extract numeric part
        var numberStr = new string(frequency.Where(char.IsDigit).ToArray());
        if (!int.TryParse(numberStr, out int number))
            return TimeSpan.Zero;

        if (frequency.EndsWith("h"))
        {
            return TimeSpan.FromHours(number);
        }
        else if (frequency.EndsWith("m"))
        {
            return TimeSpan.FromMinutes(number);
        }
        else
        {
            // Default to hours if no unit
            return TimeSpan.FromHours(number);
        }
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

                var lastSent = reminder.LastSentAt ?? reminder.CreatedAt;
                if ((now - lastSent) >= frequencyTimeSpan)
                {
                    new ReminderPublisher().PublishReminder(reminder);
                    reminder.LastSentAt = now;
                    Console.WriteLine($"[Due] Reminder for {reminder.MedicationName} (Patient {reminder.PatientId}) is due.");
                }
            }

            await _context.SaveChangesAsync();

            // Wait 1 minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
