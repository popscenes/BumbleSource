using System;
using Website.Infrastructure.Domain;
using Website.Domain.Browser;

namespace Website.Domain.Content
{
    public static class ImageInterfaceExtensions
    {
        public static void CopyFieldsFrom(this ImageInterface target, ImageInterface source)
        {
            EntityInterfaceExtensions.CopyFieldsFrom(target, source);
            target.BrowserId = source.BrowserId;
            target.Title = source.Title;
            target.Status = source.Status;
            target.Location = source.Location != null ? new Location.Location(source.Location) : null;
            target.ExternalId = source.ExternalId;
        }
    }

    public interface ImageInterface :
        EntityInterface, AggregateRootInterface, BrowserIdInterface
    {
        string Title { get; set; }
        ImageStatus Status { get; set; }
        Location.Location Location { get; set; }
        String ExternalId { get; set; }
    }
}
