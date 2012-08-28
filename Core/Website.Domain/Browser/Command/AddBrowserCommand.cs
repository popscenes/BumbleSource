using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    public class AddBrowserCommand : DefaultCommandBase
    {
        public Browser Browser { get; set; }
    }
}
