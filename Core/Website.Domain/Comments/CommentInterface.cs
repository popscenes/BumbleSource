using System;
using WebSite.Infrastructure.Domain;
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
    }

    public interface CommentInterface : EntityInterface, BrowserIdInterface, AggregateInterface
    {
        string CommentContent { get; set; }
        DateTime CommentTime { get; set; }
    }
}