using Website.Domain.Command;

namespace Website.Domain.Content.Command
{
    public class CreateImageCommand : DomainCommandBase
    {
        public string BrowserId { get; set; }
        public Content Content { get; set; }
        public string Title { get; set; }
        public Location.Location Location { get; set; }

        public string ExternalId { get; set; }
    }
}