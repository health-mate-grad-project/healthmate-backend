using System;

namespace healthmate_backend.Models
{
    public class AdminLog
    {
        public int Id { get; set; }
        public int AdminId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }
} 