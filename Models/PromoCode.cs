namespace healthmate_backend.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public class PromoCode
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int UsageLimit { get; set; } = 1;
        public int UsedCount { get; set; } = 0;
        public string Description { get; set; }
        // Foreign key to Patient
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        // One-time static method for generating promo codes for existing patients
        public static void GenerateForExistingPatients(AppDbContext context)
        {
            var patients = context.Patients
                .Include(p => p.Appointments)
                .Include(p => p.PromoCodes)
                .ToList();

            int updated = 0;
            foreach (var patient in patients)
            {
                int completedCount = patient.Appointments.Count(a => a.Status == "Completed");
                if (completedCount >= 5)
                {
                    patient.IsLoyalCustomer = true;
                    int promoCodesToAdd = completedCount / 5 - patient.PromoCodes.Count;
                    for (int i = 0; i < promoCodesToAdd; i++)
                    {
                        var promoCode = new PromoCode
                        {
                            Code = $"LOYAL-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                            ExpiryDate = DateTime.UtcNow.AddMonths(1),
                            UsageLimit = 1,
                            UsedCount = 0,
                            Description = "Loyalty Discount: Use this code for a special discount!",
                            PatientId = patient.Id,
                            Patient = patient
                        };
                        context.PromoCodes.Add(promoCode);
                    }
                    updated++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"Updated {updated} patients with loyalty status and promo codes.");
        }
    }
} 