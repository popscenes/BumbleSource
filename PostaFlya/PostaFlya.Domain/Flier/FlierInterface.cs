using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Likes;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier
{
    public enum FlierSortOrder
    {
        CreatedDate,
        EffectiveDate,
        Popularity
    }
    public static class FlierInterfaceExtensions
    {
        public static void CopyFieldsFrom(this FlierInterface target, FlierInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            target.Title = source.Title;
            target.Description = source.Description;
            target.Tags = new Tags(source.Tags);
            target.Location = new Location.Location(source.Location);
            target.Image = source.Image;
            target.BrowserId = source.BrowserId;
            target.EffectiveDate = source.EffectiveDate;
            target.CreateDate = source.CreateDate;
            target.FlierBehaviour = source.FlierBehaviour;
            target.Status = source.Status;
            target.ImageList = source.ImageList;
            target.ExternalSource = source.ExternalSource;
            target.ExternalId = source.ExternalId;
        }        
    }
    public interface FlierInterface : 
        EntityInterface, 
        BrowserIdInterface, 
        LikeableInterface,
        CommentableInterface
    {
        string Title { get; set; }
        string Description { get; set; }
        Tags Tags { get; set; }
        Location.Location Location { get; set; }
        Guid? Image { get; set; }
        DateTime EffectiveDate { get; set; }
        DateTime CreateDate { get; set; }
        FlierStatus Status { get; set; }
        FlierBehaviour FlierBehaviour { get; set; }
        List<FlierImage> ImageList { get; set; }
        string ExternalSource { get; set; }
        string ExternalId { get; set; }
        string CountryCode { get; set; }
        string PostCode { get; set; }
    }
}