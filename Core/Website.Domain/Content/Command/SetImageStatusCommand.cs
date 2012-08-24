using Website.Domain.Command;

namespace Website.Domain.Content.Command
{
    public class SetImageStatusCommand : DomainCommandBase
    {
        public string Id { get; set; }
        public ImageStatus Status { get; set; }
    }
}