using PostaFlya.Domain.Command;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Comments.Command
{
    public class CreateCommentCommand : DomainCommandBase
    {
        public string BrowserId { get; set; }
        public string Comment { get; set; }
        public EntityInterface CommentEntity { get; set; }
    }
}