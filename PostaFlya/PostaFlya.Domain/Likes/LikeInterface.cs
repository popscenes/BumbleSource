using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Flier;

namespace PostaFlya.Domain.Likes
{
    public static class LikeInterfaceExtensions
    {
        public static void CopyFieldsFrom(this LikeInterface target, LikeInterface source)
        {
            target.LikeContent = source.LikeContent;
            target.EntityId = source.EntityId;
            target.ILike = source.ILike;
            target.BrowserId = source.BrowserId;
            target.LikeTime = source.LikeTime;
            target.EntityTypeTag = source.EntityTypeTag;
        }
    }
    public interface LikeInterface : BrowserIdInterface
    {
        string EntityTypeTag { get; set; } 
        string EntityId { get; set; }
        string LikeContent { get; set; }
        bool ILike { get; set; }
        DateTime LikeTime { get; set; }
    }
}
