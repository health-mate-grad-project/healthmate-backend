namespace healthmate_backend.Models
{
    using System.Collections.Generic;

    public class Patient : User
    {
        public float Height { get; set; }
        public float Weight { get; set; }
        public required string BloodType { get; set; }
        public DateTime Birthdate { get; set; }
        public required string Location { get; set; } 

        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public string? FcmToken { get; set; }
        public bool IsLoyalCustomer { get; set; } = false;
        public ICollection<PromoCode> PromoCodes { get; set; } = new List<PromoCode>();
    }
}