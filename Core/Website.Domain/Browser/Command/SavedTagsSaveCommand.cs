using Website.Domain.Command;
using Website.Domain.Tag;

namespace Website.Domain.Browser.Command
{
    public class SavedTagsSaveCommand : DomainCommandBase
    {
        public string BrowserId { get; set; }
        public Tags Tags { get; set; }
    }
}