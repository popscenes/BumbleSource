using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    public class SetLocationCommand : DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public Location.Location Location { get; set; }
    }
}