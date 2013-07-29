using Website.Infrastructure.Command;
using Website.Domain.Content.Command;

namespace Website.Application.Domain.Content.Command
{
    public class ImageProcessSetMetaDataCommand : CommandInterface
    {
        public SetImageMetaDataCommand InitiatorCommand { get; set; }
        public string MessageId { get; set; }
    }
}