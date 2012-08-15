using PostaFlya.Domain.Command;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Content.Command
{
    public class CreateImageCommand : DomainCommandBase
    {
        public string BrowserId { get; set; }
        public Content Content { get; set; }
        public string Title { get; set; }
        public Location.Location Location { get; set; }
    }
}