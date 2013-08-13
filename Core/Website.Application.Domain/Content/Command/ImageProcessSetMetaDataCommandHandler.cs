using System;
using System.IO;
using System.Linq;
using Website.Application.Content;
using Website.Application.Extension.Content;
using Website.Domain.Content;
using Website.Domain.Location;
using Website.Infrastructure.Messaging;
using Image = System.Drawing.Image;

namespace Website.Application.Domain.Content.Command
{
    public class ImageProcessSetMetaDataCommandHandler : MessageHandlerInterface<ImageProcessSetMetaDataCommand>
    {
        private readonly BlobStorageInterface _blobStorage;

        public ImageProcessSetMetaDataCommandHandler(BlobStorageInterface blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public object Handle(ImageProcessSetMetaDataCommand command)
        {
            var imgId = command.InitiatorCommand.Id;
            var loc = command.InitiatorCommand.Location;
            var title = command.InitiatorCommand.Title;

            var q = from ts in Enum.GetValues(typeof(ThumbSize)).OfType<ThumbSize>()
                    from to in Enum.GetValues(typeof(ThumbOrientation)).OfType<ThumbOrientation>()
                    select imgId + ImageUtil.GetIdFileExtension(to, ts);
            var list = q.ToList();
            list.Add(imgId + ImageUtil.GetIdFileExtension());

            foreach (var imgid in list)
            {
                var imgBytes = _blobStorage.GetBlob(imgid);
                using (var ms = new MemoryStream(imgBytes))
                {
                    using(var img = Image.FromStream(ms))
                    {
                        var exif = new ExifImage(img);
                        if (!string.IsNullOrWhiteSpace(title))
                            exif.SetImageTitle(title);
                        if (loc != null && loc.IsValid())
                        {
                            exif.SetLatitude(loc.Latitude);
                            exif.SetLongitude(loc.Longitude);
                        }
                        _blobStorage.SetBlob(imgid, img.GetBytes()); 
                    }
                }
            }

            return command;
        }

    }
}
