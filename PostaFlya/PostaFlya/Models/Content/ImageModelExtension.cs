using PostaFlya.Models.Location;
using Website.Application.Content;
using Website.Application.Domain.Content;

namespace PostaFlya.Models.Content
{
    public static class ImageModelExtension
    {
        public static ImageViewModel ToViewModel(this Website.Domain.Content.ImageInterface image)
        {
            return new ImageViewModel()
                       {
                           BrowserId = image.BrowserId,
                           ImageId = image.Id,
                           Location = image.Location.ToViewModel(),
                           Title = image.Title,
                           Status = image.Status.ToString()
                       }; 
        }

        public static ImageViewModel GetImageUrl(this ImageViewModel model, BlobStorageInterface blobStorage, bool fromCdn = true)
        {
            var uri = blobStorage.GetBlobUri(model.ImageId + ImageUtil.GetIdFileExtension(), fromCdn);
            if (uri == null) return model;
            model.ImageUrl = uri.GetThumbUrlForImage(ThumbOrientation.Vertical, ThumbSize.S100);
            return model;
        } 
    }
}