using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Browser;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Content
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
        }
    }

    public interface ImageInterface : 
        EntityInterface, BrowserIdInterface
    {
        string Title { get; set; }
        ImageStatus Status { get; set; }
        Location.Location Location { get; set; }
    }
}
