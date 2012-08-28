using System.Runtime.Caching;
using Website.Application.Binding;
using Website.Application.Caching;
using Website.Application.Caching.Command;
using Website.Infrastructure.Command;

namespace PostaFlya.Application.Domain.Behaviour.TaskJob
{
    public static class CachedTaskJobContext
    {
        public const string Region = "taskjob";
        public const string TaskJob = "tsk";
        public const string Bids = "bid";
    }
}