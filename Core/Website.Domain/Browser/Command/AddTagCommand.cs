using Website.Infrastructure.Command;
using Website.Domain.Tag;

namespace Website.Domain.Browser.Command
{
    public class AddTagCommand : DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public Tags Tags { get; set; }
    }
}