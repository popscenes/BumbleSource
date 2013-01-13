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

    [Serializable]
    public class RepeatJob : JobBase, RepeatJobInterface
    {
        public int RepeatSeconds { get; set; }
        public override void CalculateNextRunFromNow(TimeServiceInterface  timeService)
        { 
            var curr = timeService.GetCurrentTime();
            NextRun = curr + TimeSpan.FromSeconds(RepeatSeconds);
        }
        public override void CopyState<JobBaseType>(JobBaseType source)
        {
            this.CopyFieldsFrom(source as RepeatJobInterface);
        }
    }
}