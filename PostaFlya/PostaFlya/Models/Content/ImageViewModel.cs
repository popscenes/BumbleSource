using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Models.Location;
using WebSite.Application.Content;
using Website.Application.Domain.Content;

namespace PostaFlya.Models.Content
{
    public class ImageViewModel
    {
        public string BrowserId { get; set; }
        public string ImageId { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }


        public String Status { get; set; }
        public LocationModel Location { get; set; }
    }

    public static class ImageViewModelExtensions
    {
        public static ImageViewModel GetDefaultImageUrl(this ImageViewModel model, BlobStorageInterface blobStorage, ThumbOrientation orientation = ThumbOrientation.Horizontal, ThumbSize thumbSize = ThumbSize.S450)
        {
            var uri = blobStorage.GetBlobUri(model.ImageId.ToString());
            if (uri == null) return model;
            model.ImageUrl = uri.GetUrlForImage(orientation, thumbSize);
            return model;
        }
    }
}