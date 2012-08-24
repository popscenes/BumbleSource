using Website.Domain.Command;

namespace Website.Domain.Browser.Command
{
    public class SetLocationCommand : DomainCommandBase
    {
        public string BrowserId { get; set; }
        public Location.Location Location { get; set; }
    }
}