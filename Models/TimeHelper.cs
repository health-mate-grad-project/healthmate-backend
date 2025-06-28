using System;

namespace healthmate_backend.Models
{
    public static class TimeHelper
    {
        public static DateTime ToEgyptTime(DateTime utcDateTime)
        {
            var egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), egyptZone);
        }

        public static DateTime FromEgyptTime(DateTime egyptDateTime)
        {
            var egyptZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(egyptDateTime, egyptZone);
        }
    }
} 