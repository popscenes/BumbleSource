using PostaFlya.Application.Domain.Content;
using PostaFlya.Models.Location;
using WebSite.Application.Content;

namespace PostaFlya.Models.Content
{
    public static class ImageModelExtension
    {
        public static ImageViewModel ToViewModel(this Domain.Content.ImageInterface image)
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

        public static ImageViewModel GetImageUrl(this ImageViewModel model, BlobStorageInterface blobStorage)
        {
            var uri = blobStorage.GetBlobUri(model.ImageId);
            if (uri == null) return model;
            model.ImageUrl = uri.GetUrlForImage(ThumbOrientation.Vertical, ThumbSize.S100);
            return model;
        } 
    }
}