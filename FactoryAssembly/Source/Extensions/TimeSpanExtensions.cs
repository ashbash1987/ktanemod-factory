using System;

namespace FactoryAssembly
{
    public static class TimeSpanExtensions
    {
        public static string GetBombTime(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 60.0f)
            {
                return $"{timeSpan.TotalSeconds:00.00}";
            }
            else if (timeSpan.TotalMinutes < 60.0f)
            {
                return $"{timeSpan.Minutes}:{timeSpan.Seconds:00}";
            }
            else
            {
                return $"{(int)Math.Floor(timeSpan.TotalHours)}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }
        }
    }
}
