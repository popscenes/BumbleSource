using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Likes
{
    public static class LikeInterfaceExtensions
    {
        public static string GetId(this LikeInterface target)
        {
            return target.AggregateId + target.BrowserId;
        }

        public static void CopyFieldsFrom(this LikeInterface target, LikeInterface source)
        {
            target.LikeContent = source.LikeContent;
            target.AggregateId = source.AggregateId;
            target.ILike = source.ILike;
            target.BrowserId = source.BrowserId;
            target.LikeTime = source.LikeTime;
            target.EntityTypeTag = source.EntityTypeTag;
        }
    }
    public interface LikeInterface : EntityInterface
        , BrowserIdInterface
        , AggregateInterface
    {
        string EntityTypeTag { get; set; } 
        string LikeContent { get; set; }
        bool ILike { get; set; }
        DateTime LikeTime { get; set; }
    }
}
