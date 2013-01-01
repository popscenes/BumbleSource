using System;
using Website.Infrastructure.Command;

namespace Website.Application.Schedule.Command
{
    [Serializable]
    public class JobCommand : DefaultCommandBase
    {
        public JobBase JobBase { get; set; }
    }
}