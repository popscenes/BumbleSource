using Website.Domain.Command;
using Website.Domain.Tag;

namespace Website.Domain.Browser.Command
{
    public class SavedTagsDeleteCommand : DomainCommandBase
    {
        public Tags Tags { get; set; }
        public string BrowserId { get; set; }

    }
}