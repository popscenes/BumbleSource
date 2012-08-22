using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Browser;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Likes
{
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
