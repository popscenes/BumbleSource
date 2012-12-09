using System;
using Website.Infrastructure.Domain;

namespace Website.Domain.Comments
{
    [Serializable]
    public class Comment : EntityBase<CommentInterface>, CommentInterface
    {
        public string AggregateId { get; set; }
        public string CommentContent { get; set; }
        public DateTime CommentTime { get; set; }
        public string BrowserId { get; set; }
        public string AggregateTypeTag { get; set; } 
    }
}