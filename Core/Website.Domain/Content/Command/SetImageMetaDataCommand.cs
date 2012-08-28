using Website.Infrastructure.Command;

namespace Website.Domain.Content.Command
{
    public class SetImageMetaDataCommand : DefaultCommandBase
    {
        public string Id { get; set; }
        public Location.Location Location { get; set; }
        public string Title { get; set; }
    }
}
