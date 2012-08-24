using Website.Domain.Command;

namespace Website.Domain.Browser.Command
{
    public class SetDistanceCommand: DomainCommandBase
    {
        public string BrowserId {get; set; }
        public int Distance {get; set; }
    }
}