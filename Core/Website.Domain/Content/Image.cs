using System;
using WebSite.Infrastructure.Domain;

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

        #endregion
    }
}
