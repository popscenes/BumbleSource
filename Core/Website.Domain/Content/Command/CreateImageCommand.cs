using Website.Infrastructure.Command;

namespace Website.Domain.Content.Command
{
    public class CreateImageCommand : DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public Content Content { get; set; }
        public string Title { get; set; }
        public Location.Location Location { get; set; }
        public bool Anonymous { get; set; }
        public string ExternalId { get; set; }
        public bool KeepFileImapeType {get; set; }
    }
}