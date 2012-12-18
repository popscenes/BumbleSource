using Website.Infrastructure.Command;

namespace Website.Application.Schedule.Command
{
    public class JobCommand : DefaultCommandBase
    {
        public JobBase JobBase { get; set; }
    }
}