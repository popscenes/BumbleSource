using System;
using WebSite.Infrastructure.Domain;

namespace Website.Domain.Likes
{
    [Serializable]
    public class Like : EntityBase<LikeInterface>, LikeInterface 
    {
        public string EntityTypeTag{ get; set; }
        public string AggregateId { get; set; }
        public string BrowserId { get; set; }
        public string LikeContent { get; set; }
        public bool ILike { get; set; }
        public DateTime LikeTime { get; set; }
    }
}
