using System.Collections.Generic;
using System.Linq;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Content;
using Website.Common.Model;
using Website.Domain.Content;

namespace PostaFlya.Areas.WebApi.Flyers.Model
{
    public class ToImageModel : ViewModelMapperInterface<ImageModel, Image>
    {
        private readonly BlobStorageInterface _blobStorage;

        public ToImageModel([ImageStorage]BlobStorageInterface blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public ImageModel ToViewModel(ImageModel target, Image source)
        {
            if(target == null)
                target = new ImageModel();

            if (source.AvailableDimensions == null) return null;
            
            target.BaseUrl = _blobStorage.GetBlobUri(source.Id).ToString();
            target.Extensions = source.AvailableDimensions
                .Select(d => new ImageModel.Extension()
                {
                    Width = d.Width,
                    Height = d.Height,
                    ScaleAxis = d.Orientation.ToString(),
                    UrlExtension = d.UrlExtension
                    
                }).OrderBy(extension => extension.Width)
                .ToList();

            return target;
        }
    }

    public class ImageModel : IsModelInterface
    {
        public ImageModel()
        {
            Extensions = new List<Extension>();
        }

        public class Extension
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public string UrlExtension { get; set; }
            public string ScaleAxis { get; set; }
        }
        public string BaseUrl { get; set; }
        public List<Extension> Extensions { get; set; }

        public string UrlForAxisWidth(string scale, int width)
        {
            var ext = Extensions.FirstOrDefault(extension => extension.ScaleAxis == scale && extension.Width >= width);
            return BaseUrl + ext.UrlExtension;
        }

    }
}