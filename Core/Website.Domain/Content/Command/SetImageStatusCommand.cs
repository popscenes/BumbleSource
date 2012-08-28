using Website.Infrastructure.Command;

namespace Website.Domain.Content.Command
{
    public class SetImageStatusCommand : DefaultCommandBase
    {
        public string Id { get; set; }
        public ImageStatus Status { get; set; }
    }
}