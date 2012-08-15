using PostaFlya.Domain.Comments.Command;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Likes.Command;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Flier.Command
{
    internal interface FlierRepositoryInterface : GenericRepositoryInterface<FlierInterface>
        , AddLikeInterface<FlierInterface>
        , AddCommentInterface<FlierInterface>
    {

    }
}