using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using healthmate_backend.Models;

public class ReminderPublisher
{
    public void PublishReminder(Reminder reminder)
    {
        try
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "reminder_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var payload = new
            {
                patientId = reminder.PatientId,
                medicationName = reminder.MedicationName,
                dosage = reminder.Dosage,
                notes = reminder.Notes,
                notificationMessage = $"Reminder: Take {reminder.Dosage}mg of {reminder.MedicationName}. Notes: {reminder.Notes}"
            };

            string json = JsonSerializer.Serialize(payload);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "",
                routingKey: "reminder_queue",
                basicProperties: null,
                body: body);

            Console.WriteLine("[x] Reminder published to RabbitMQ.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Failed to publish reminder: {ex.Message}");
        }
    }
}