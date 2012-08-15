using PostaFlya.Domain.Command;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Likes.Command
{
    public class LikeCommand : DomainCommandBase
    {
        public string Comment { get; set; }
        public EntityInterface LikeEntity { get; set; }
        public string BrowserId { get; set; }
        public bool ILike { get; set; }
    }
}