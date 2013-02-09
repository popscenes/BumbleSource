using System;
using System.Linq;

namespace Website.Application.Schedule
{
    public static class AbsoluteRepeatJobInterfaceExtensions
    {
        public static void CopyFieldsFrom(this AbsoluteRepeatJobInterface target, AbsoluteRepeatJobInterface source)
        {
            JobInterfaceExtensions.CopyFieldsFrom(target, source);
            target.DayOfWeek = source.DayOfWeek;
            target.HourOfDay = source.DayOfWeek;
            target.Minute = source.DayOfWeek;
        }
    }

    public interface AbsoluteRepeatJobInterface : JobInterface
    {
        string DayOfWeek { get; set; }
        string HourOfDay { get; set; }
        string Minute { get; set; }
    }

    public class AbsoluteRepeatJob : JobBase, AbsoluteRepeatJobInterface
    {
        public const string All = "*";
        public string DayOfWeek { get; set; }// (null invalid in case we add) or * for all, or DayOfWeek.Monday + "," DayOfWeek.Tuesday 
        public string HourOfDay { get; set; }// null for 0 am * for every hour
        public string Minute { get; set; }// null for 0 minutes * for every minute

        public static string GetDaysOfWeekStringFor(params DayOfWeek[] days)
        {
            var ret = "";
            foreach (var dayOfWeek in days)
            {
                if (!string.IsNullOrEmpty(ret))
                    ret += ",";
                ret += dayOfWeek;
            }
            return ret;
        }
        public override void CalculateNextRunFromNow(TimeServiceInterface timeService)
        {
            var curr = timeService.GetCurrentTime();

            NextRun = GetNextRun(curr);
        }

        public override void CopyState<JobBaseType>(JobBaseType source)
        {
            this.CopyFieldsFrom(source as AbsoluteRepeatJobInterface);
        }

        private DateTimeOffset GetNextRun(DateTimeOffset curr)
        {
            var current = new DateTimeOffset(curr.Year, curr.Month, curr.Day, curr.Hour, curr.Minute + 1, 0, curr.Offset);

            //not efficient but hey only runs when job runs, and a simple implementation!
            while (!Satisfies(current))
            {
                current = current.AddMinutes(1);
            }

            return current;
        }


        private bool Satisfies(DateTimeOffset current)
        {
            return IsDayOk(current) && IsHourOk(current) && IsMinuteOk(current);
        }

        private bool IsMinuteOk(DateTimeOffset current)
        {
            if (Minute != null && Minute.Equals(All))
                return true;

            var min = Minute != null  ? Int32.Parse(Minute) : 0;
            return current.Minute == min;
        }

        private bool IsHourOk(DateTimeOffset current)
        {
            if (HourOfDay != null && HourOfDay.Equals(All))
                return true;
            var hour = HourOfDay != null ? Int32.Parse(HourOfDay) : 0;
            return current.Hour == hour;
        }

        private bool IsDayOk(DateTimeOffset current)
        {
            if (DayOfWeek == null || DayOfWeek.Equals(All))//different to others as we may support day in month one day
                return true;

            var days = DayOfWeek.Split(',').Select(s => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), s)).OrderBy(day => day).ToList();
            return (days.Any(day => day == current.DayOfWeek));
        }
    }
}
