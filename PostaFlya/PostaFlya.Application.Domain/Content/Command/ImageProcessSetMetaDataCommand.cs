using PostaFlya.Domain.Content.Command;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Application.Domain.Content.Command
{
    public class ImageProcessSetMetaDataCommand : CommandInterface
    {
        public SetImageMetaDataCommand InitiatorCommand { get; set; }
        public string CommandId { get; set; }
    }
}