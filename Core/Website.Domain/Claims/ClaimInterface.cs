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
            target.ClaimContext = source.ClaimContext;
            target.AggregateId = source.AggregateId;
            target.BrowserId = source.BrowserId;
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
        DateTime ClaimTime { get; set; }
    }
}
