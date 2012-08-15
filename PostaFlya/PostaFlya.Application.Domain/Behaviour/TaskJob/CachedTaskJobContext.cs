using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching;
using WebSite.Application.Caching.Command;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Application.Domain.Behaviour.TaskJob
{
    public static class CachedTaskJobContext
    {
        public const string Region = "taskjob";
        public const string TaskJob = "tsk";
        public const string Bids = "bid";
    }
}