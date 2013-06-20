using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Location;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Content;
using Website.Common.Model;

namespace PostaFlya.Models.Content
{
    public class ToImageViewModel : ViewModelMapperInterface<ImageViewModel, FlierImage>
    {
        private readonly BlobStorageInterface _blobStorage;

        public ToImageViewModel([ImageStorage]BlobStorageInterface blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public ImageViewModel ToViewModel(ImageViewModel target, FlierImage source)
        {
            if(target == null)
                target = new ImageViewModel();

            var uri = _blobStorage.GetBlobUri(source.ImageID + ImageUtil.GetIdFileExtension());
            if (uri != null)
                target.ImageUrl = uri.ToString();
            
            return target;
        }
    }
    public class ImageViewModel
    {
        public string BrowserId { get; set; }
        public string ImageId { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }


        public String Status { get; set; }
        public LocationModel Location { get; set; }
    }

//    public static class ImageViewModelExtensions
//    {
//        public static ImageViewModel GetImageUrl(this ImageViewModel model, BlobStorageInterface blobStorage, ThumbOrientation orientation = ThumbOrientation.Horizontal, ThumbSize thumbSize = ThumbSize.S456)
//        {
//            var uri = blobStorage.GetBlobUri(model.ImageId + ImageUtil.GetIdFileExtension());
//            if (uri == null) return model;
//            model.ImageUrl = uri.GetThumbUrlForImage(orientation, thumbSize);
//            return model;
//        }
//    }
}