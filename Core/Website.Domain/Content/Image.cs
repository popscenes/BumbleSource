using System;
using System.Collections.Generic;
using Website.Infrastructure.Domain;

namespace Website.Domain.Content
{
    [Serializable]
    public class Image : EntityBase<ImageInterface>, ImageInterface
    {
        #region Implementation of ImageInterface

        public string BrowserId { get; set; }
        public string Title { get; set; }

        public ImageStatus Status { get; set; }
        public Location.Location Location { get; set; }

        public string ExternalId { get; set; }
        public List<ImageDimension> AvailableDimensions { get; set; }

        #endregion
    }

    public struct ImageDimension
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string UrlExtension { get; set; }
        public ThumbOrientation Orientation { get; set; }
    }

    public enum ThumbOrientation
    {
        Horizontal,
        Vertical,
        Original,
        Square
    }
}
