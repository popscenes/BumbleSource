using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Content;
using Website.Common.Model;
using Website.Domain.Content;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
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
            
            target.BaseUrl = _blobStorage.GetBlobUri(source.Id).ToString();
            target.Extensions = source.AvailableDimensions
                .Where(dimension => 
                    (dimension.Orientation == 
                    ThumbOrientation.Square
                    && (dimension.Width == (int) ThumbSize.S150 || dimension.Width == (int) ThumbSize.S300))
                    ||
                    (dimension.Orientation == ThumbOrientation.Horizontal && dimension.Width == (int) ThumbSize.S450)
                    ||
                    (dimension.UrlExtension == ImageUtil.GetIdFileExtension())
                    )
                .Select(d => new ImageModel.Extension()
                {
                    Width = d.Width,
                    Height = d.Height,
                    ScaleAxis = d.Orientation.ToString(),
                    UrlExtension = d.UrlExtension
                    
                }).OrderBy(extension => extension.Height)
                .ToList();

            return target;
        }
    }

    public class ImageModel : IsModelInterface
    {
        public class Extension
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public string UrlExtension { get; set; }
            public string ScaleAxis { get; set; }
        }
        public string BaseUrl { get; set; }
        public List<Extension> Extensions { get; set; }

    }
}