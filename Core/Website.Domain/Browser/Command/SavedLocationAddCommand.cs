using Website.Domain.Command;

namespace Website.Domain.Browser.Command
{
    public class SavedLocationAddCommand: DomainCommandBase
    {
        public string BrowserId { get; set; }
        public Location.Location Location { get; set; }
    }
}