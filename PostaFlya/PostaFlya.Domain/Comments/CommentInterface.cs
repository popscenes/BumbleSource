using System;
using PostaFlya.Domain.Browser;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Comments
{
    public static class CommentInteraceExtensions
    {
        public static void CopyFieldsFrom(this CommentInterface target, CommentInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            target.CommentContent = source.CommentContent;
            target.EntityId = source.EntityId;
            target.BrowserId = source.BrowserId;
            target.CommentTime = source.CommentTime;
        }
    }

    public interface CommentInterface : EntityInterface, BrowserIdInterface
    {
        string EntityId { get; set; }
        string CommentContent { get; set; }
        DateTime CommentTime { get; set; }
    }
}