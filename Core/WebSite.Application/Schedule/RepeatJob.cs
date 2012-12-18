using System;

namespace Website.Application.Schedule
{
    public static class RepeatJobInterfaceExtensions
    {
        public static void CopyFieldsFrom(this RepeatJobInterface target, RepeatJobInterface source)
        {
            JobInterfaceExtensions.CopyFieldsFrom(target, source);
            target.RepeatSeconds = source.RepeatSeconds;
        }
    }
    public interface RepeatJobInterface : JobInterface
    {
        int RepeatSeconds { get; set; }
    }

    public class RepeatJob : JobBase, RepeatJobInterface
    {
        public int RepeatSeconds { get; set; }
        public override void CalculateNextRun(TimeServiceInterface  timeService)
        { 
            var curr = timeService.GetCurrentTime();
            if (LastRun == default(DateTimeOffset))
                LastRun = curr;

            NextRun = LastRun + TimeSpan.FromSeconds(RepeatSeconds);
            if (NextRun < curr)
                NextRun = curr;
        }
        public override void CopyState<JobBaseType>(JobBaseType source)
        {
            this.CopyFieldsFrom(source as RepeatJobInterface);
        }
    }
}