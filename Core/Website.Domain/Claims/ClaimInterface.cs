using System;
using Website.Infrastructure.Domain;
using Website.Domain.Browser;

namespace Website.Domain.Claims
{
    public static class ClaimInterfaceExtensions
    {
        public static string GetId(this ClaimInterface target)
        {
            return target.AggregateId + target.BrowserId;
        }

        public static void CopyFieldsFrom(this ClaimInterface target, ClaimInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            BrowserIdInterfaceExtensions.CopyFieldsFrom(target, source);
            AggregateInterfaceExtensions.CopyFieldsFrom(target, source);
            target.ClaimContext = source.ClaimContext;
            target.ClaimMessage = source.ClaimMessage;
            target.ClaimTime = source.ClaimTime;
            target.EntityTypeTag = source.EntityTypeTag;
        }
    }
    public interface ClaimInterface : EntityInterface
        , BrowserIdInterface
        , AggregateInterface
    {
        string EntityTypeTag { get; set; } 
        string ClaimContext { get; set; }
        string ClaimMessage { get; set; }
        DateTime ClaimTime { get; set; }
    }
}
