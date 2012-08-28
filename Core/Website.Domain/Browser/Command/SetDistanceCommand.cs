using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    public class SetDistanceCommand: DefaultCommandBase
    {
        public string BrowserId {get; set; }
        public int Distance {get; set; }
    }
}