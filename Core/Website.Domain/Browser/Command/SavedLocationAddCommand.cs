using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    public class SavedLocationAddCommand: DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public Location.Location Location { get; set; }
    }
}