using Website.Infrastructure.Command;
using Website.Domain.Tag;

namespace Website.Domain.Browser.Command
{
    public class SavedTagsDeleteCommand : DefaultCommandBase
    {
        public Tags Tags { get; set; }
        public string BrowserId { get; set; }

    }
}