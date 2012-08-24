using WebSite.Infrastructure.Domain;
using Website.Domain.Command;

namespace Website.Domain.Comments.Command
{
    public class CreateCommentCommand : DomainCommandBase
    {
        public string BrowserId { get; set; }
        public string Comment { get; set; }
        public EntityInterface CommentEntity { get; set; }
    }
}