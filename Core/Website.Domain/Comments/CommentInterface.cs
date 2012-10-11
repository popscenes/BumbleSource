using System;
using Website.Infrastructure.Domain;
using Website.Domain.Browser;

namespace Website.Domain.Comments
{
    public static class CommentInterfaceExtensions
    {
        public static void CopyFieldsFrom(this CommentInterface target, CommentInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            target.CommentContent = source.CommentContent;
            target.AggregateId = source.AggregateId;
            target.BrowserId = source.BrowserId;
            target.CommentTime = source.CommentTime;
        }

        //var dattimedesc = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("D20");
        //var dattimeasc = (DateTime.UtcNow.Ticks).ToString("D20");
        public static void SetId(this CommentInterface target)
        {
            target.Id = target.CommentTime.Ticks.ToString("D20") + Guid.NewGuid();
        }
    }

    public interface CommentInterface : EntityInterface, BrowserIdInterface, AggregateInterface
    {
        string CommentContent { get; set; }
        DateTime CommentTime { get; set; }
    }
}