using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Domain.Comments.Command
{
    public class CreateCommentCommand : DefaultCommandBase
    {
        public string BrowserId { get; set; }
        public string Comment { get; set; }
        public EntityInterface CommentEntity { get; set; }
    }
}