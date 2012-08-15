using PostaFlya.Domain.Command;

namespace PostaFlya.Domain.Content.Command
{
    public class SetImageStatusCommand : DomainCommandBase
    {
        public string Id { get; set; }
        public ImageStatus Status { get; set; }
    }
}