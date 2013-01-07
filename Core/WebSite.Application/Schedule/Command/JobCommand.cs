using System;
using Website.Infrastructure.Command;

namespace Website.Application.Schedule.Command
{
    [Serializable]
    public class JobCommand : DefaultCommandBase
    {
        public string JobId { get; set; }
        public Type JobType { get; set; }
    }
}