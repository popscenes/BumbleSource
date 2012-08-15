using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Comments.Command
{
    internal interface AddCommentInterface
    {
        CommentableInterface AddComment(CommentInterface comment);
    }

    internal interface AddCommentInterface<EntityType>
        : AddCommentInterface where EntityType : CommentableInterface
    {
    }
}