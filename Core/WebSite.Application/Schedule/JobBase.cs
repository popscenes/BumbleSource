﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            target.CurrentProcessor = source.CurrentProcessor;
            if(source.JobStorage != null)
                target.JobStorage = new Dictionary<string, string>(source.JobStorage);
        }
    }
    public interface JobInterface : AggregateRootInterface
    {
        DateTimeOffset LastRun { get; set; }
        TimeSpan LastDuration { get; set; }
        DateTimeOffset NextRun { get; set; }
        bool InProgress { get; set; }
        Type JobActionClass { get; set; }
        void CalculateNextRunFromNow(TimeServiceInterface timeService);
        bool IsRunDue(TimeServiceInterface timeService);
        Dictionary<string, string> JobStorage { get; set; }
        Guid CurrentProcessor { get; set; }
    }

    [Serializable]
    public class JobBase : JobInterface
    {
        public JobBase()
        {
            TimeOut = TimeSpan.FromMinutes(20);
        }
        public string Id { get; set; }
        public string FriendlyId { get; set; }
        public DateTimeOffset LastRun { get; set; }
        public TimeSpan TimeOut { get; set; }
        public TimeSpan LastDuration { get; set; }
        public DateTimeOffset NextRun { get; set; }
        public bool InProgress { get; set; }
        public Type JobActionClass { get; set; }
        public Guid CurrentProcessor { get; set; }
        public virtual void CalculateNextRunFromNow(TimeServiceInterface timeService){}

        public bool IsTimedOut(TimeServiceInterface timeService)
        {
            if (InProgress && (TimeOut != default(TimeSpan)) && (timeService.GetCurrentTime() - LastRun) > TimeOut)
            {
                Trace.TraceWarning("Job timed out and being reset assumed dead {0}", Id);
                return true;
            }
            return false;
        }

        public bool IsRunDue(TimeServiceInterface timeService)
        {
            return IsTimedOut(timeService) || timeService.GetCurrentTime() > NextRun;
        }
        public virtual void CopyState<JobBaseType>(JobBaseType source) where JobBaseType : JobInterface
        {
            //this.CopyFieldsFrom(source as JobInterface);
        }

        public Dictionary<string, string> JobStorage { get; set; }
    }

         
}