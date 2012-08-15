using System;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Comments
{
    public class Comment : EntityBase<CommentInterface>, CommentInterface
    {
        public string EntityId { get; set; }
        public string CommentContent { get; set; }
        public DateTime CommentTime { get; set; }
        public string BrowserId { get; set; }
    }
}