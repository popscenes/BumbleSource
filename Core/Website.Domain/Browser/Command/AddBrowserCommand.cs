using Website.Domain.Command;

namespace Website.Domain.Browser.Command
{
    public class AddBrowserCommand : DomainCommandBase
    {
        public Browser Browser { get; set; }
    }
}
