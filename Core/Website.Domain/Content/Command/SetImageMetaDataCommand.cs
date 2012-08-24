using Website.Domain.Command;

namespace Website.Domain.Content.Command
{
    public class SetImageMetaDataCommand : DomainCommandBase
    {
        public string Id { get; set; }
        public Location.Location Location { get; set; }
        public string Title { get; set; }
    }
}
