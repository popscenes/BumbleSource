using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Infrastructure.Util
{
    public static class DateTimeUtil
    {
        public static TimeSpan Minutes(this int minutes)
        {
            return TimeSpan.FromMinutes(minutes);
        }

        public static TimeSpan Hours(this int hours)
        {
            return TimeSpan.FromHours(hours);
        }

        public static TimeSpan Days(this int days)
        {
            return TimeSpan.FromDays(days);
        }

        public static DateTime RoundDownMinutes(this DateTime now, int minutes)
        {
            return now.Round(minutes.Minutes());
        }

        public static DateTimeOffset GetDateOnly(this DateTimeOffset offset)
        {
            return new DateTimeOffset(offset.Date, offset.Offset);
        }

        public static DateTime Round(this DateTime now, TimeSpan t)
        {
            if (t.TotalDays > 1)
                return new DateTime(now.Year, now.Month, now.Day.RoundBy((int)t.TotalDays), 0, 0, 0, now.Kind);

            if (t.TotalHours > 1)
                return new DateTime(now.Year, now.Month, now.Day, now.Hour.RoundBy((int)t.TotalHours), 0, 0, now.Kind);

            if (t.TotalMinutes > 1)
                return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute.RoundBy((int)t.TotalMinutes), 0, now.Kind);

            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second.RoundBy((int)t.TotalSeconds), now.Kind);
        }

        public static DateTimeOffset RoundDownMinutes(this DateTimeOffset now, int minutes)
        {
            return now.Round(minutes.Minutes());
        }

        public static DateTimeOffset Round(this DateTimeOffset now, TimeSpan t)
        {
            if (t.TotalDays > 1)
                return new DateTimeOffset(now.Year, now.Month, now.Day.RoundBy((int)t.TotalDays), 0, 0, 0, now.Offset);

            if (t.TotalHours > 1)
                return new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour.RoundBy((int)t.TotalHours), 0, 0, now.Offset);

            if (t.TotalMinutes > 1)
                return new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute.RoundBy((int)t.TotalMinutes), 0, now.Offset);

            return new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second.RoundBy((int)t.TotalSeconds), now.Offset);
        }

        static int RoundBy(this int value, int number)
        {
            return value / number * number;
        }
    }
}
