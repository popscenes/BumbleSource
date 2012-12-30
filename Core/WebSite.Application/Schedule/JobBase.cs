using System;
using System.Collections.Generic;
using Website.Infrastructure.Domain;

namespace Website.Application.Schedule
{
    public static class JobInterfaceExtensions 
    {
        public static void CopyFieldsFrom(this JobInterface target, JobInterface source)
        {
            EntityIdInterfaceExtensions.CopyFieldsFrom(target, source);
            target.LastRun = source.LastRun;
            target.LastDuration = source.LastDuration;
            target.NextRun = source.NextRun;
            target.InProgress = source.InProgress;
            target.JobActionClass = source.JobActionClass;
        }
    }
    public interface JobInterface : EntityIdInterface
    {
        DateTimeOffset LastRun { get; set; }
        TimeSpan LastDuration { get; set; }
        DateTimeOffset NextRun { get; set; }
        bool InProgress { get; set; }
        Type JobActionClass { get; set; }
        void CalculateNextRun(TimeServiceInterface timeService);
        bool IsRunDue(TimeServiceInterface timeService);
    }

    public class JobBase : JobInterface
    {
        public string Id { get; set; }
        public string FriendlyId { get; set; }
        public DateTimeOffset LastRun { get; set; }
        public TimeSpan LastDuration { get; set; }
        public DateTimeOffset NextRun { get; set; }
        public bool InProgress { get; set; }
        public Type JobActionClass { get; set; }
        public virtual void CalculateNextRun(TimeServiceInterface timeService){}
        public bool IsRunDue(TimeServiceInterface timeService)
        {
            return timeService.GetCurrentTime() > NextRun;
        }
        public virtual void CopyState<JobBaseType>(JobBaseType source) where JobBaseType : JobInterface{}
        public Dictionary<string, string> JobStorage { get; set; }
    }

         
}