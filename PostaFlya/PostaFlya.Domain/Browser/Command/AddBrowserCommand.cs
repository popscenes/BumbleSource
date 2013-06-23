using Website.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    public class AddBrowserCommand : DefaultCommandBase
    {
        public Browser Browser { get; set; }
    }
}
