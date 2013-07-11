using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Extension.Content;
using Website.Infrastructure.Command;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Image = System.Drawing.Image;

namespace Website.Application.Domain.Content.Command
{
    public class ImageProcessCommandHandler : CommandHandlerInterface<ImageProcessCommand>
    {
        public static readonly int MaxWidth = ImageUtil.A4300DpiSize.Width;
        public static readonly int MaxHeight = ImageUtil.A4300DpiSize.Width;


        public const double MaxAspectRatio = 2.5;


        private readonly BlobStorageInterface _blobStorage;
        private readonly CommandBusInterface _commandBus;

        public ImageProcessCommandHandler([ImageStorage] BlobStorageInterface blobStorage,
            CommandBusInterface commandBus)
        {
            _blobStorage = blobStorage;
            _commandBus = commandBus;
        }



        #region Implementation of CommandHandlerInterface<in ImageProcessCommand>

        public object Handle(ImageProcessCommand command)
        {
            using (var ms = new MemoryStream(command.ImageData, false))
            {
                try
                {
                    using(var img = Image.FromStream(ms))
                    {
                        ProcessImage(img, command);
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceInformation("ImageProcessCommandHandler Error: {0}, Stack {1}", e.Message, e.StackTrace);
                    _commandBus.Send(new SetImageStatusCommand()
                    {
                        Id = command.CommandId, // commandid == imageid
                        Status = ImageStatus.Failed
                    });
                }
                
            }

            return true;
        }

        private static ImageDimension GetDim(Image img, string urlExt, ThumbOrientation orientation)
        {
            return new ImageDimension()
                {
                    Height = img.Height,
                    Width = img.Width,
                    UrlExtension = urlExt,
                    Orientation = orientation
                };
        }

        private void ProcessImage(Image img, ImageProcessCommand command)
        {

            Image aspectImg = null;
            Image maxDimImg = null;
            try
            {
                var curr = img;
                aspectImg = EnsureAspectRatio(curr, MaxAspectRatio);
                if (aspectImg != null)
                    curr = aspectImg;

                maxDimImg = EnsureMaxiums(curr);
                if (maxDimImg != null)
                    curr = maxDimImg;

                var dims = new List<ImageDimension>(){GetDim(curr, ImageUtil.GetIdFileExtension(), ThumbOrientation.Original)};
                var convdata = curr.GetBytes();
                _blobStorage.SetBlob(command.CommandId + ImageUtil.GetIdFileExtension(), convdata, BlobProperties.JpegContentTypeDefault);

                dims.AddRange(CreateThumbs(command.CommandId, curr));

                ProcessMetaData(img, command.CommandId, dims);

                _commandBus.Send(new SetImageStatusCommand()
                                     {
                                         Id = command.CommandId, // this commandid == imageid
                                         Status = ImageStatus.Ready,
                                     });

                if (aspectImg != null)
                    aspectImg.Dispose();
                if (maxDimImg != null)
                    maxDimImg.Dispose();
            }
            catch (Exception)
            {
                if (aspectImg != null)
                    aspectImg.Dispose();
                if (maxDimImg != null)
                    maxDimImg.Dispose();
                throw;
            }
        }

        private void ProcessMetaData(Image img, string imgId, List<ImageDimension> dims)
        {
            var loc = new Website.Domain.Location.Location();
            var exif = new ExifImage(img);

            var latitude = exif.GetLatitude();
            var longitude = exif.GetLongitude();
            if (latitude.HasValue && longitude.HasValue)
            {
                loc.Latitude = latitude.Value;
                loc.Longitude = longitude.Value;
            }

            var title = exif.GetImageTitle();
            if (string.IsNullOrWhiteSpace(title))
                title = exif.GetImageDescription();

            _commandBus.Send(new SetImageMetaDataCommand()
            {
                Id = imgId,
                Location = loc,
                Title = title,
                Dimensions = dims
            });
        }

        private IEnumerable<ImageDimension> CreateThumbs(string commandId, Image img)
        {
            var ret = new List<ImageDimension>();

            var sizes = (ThumbSize[])Enum.GetValues(typeof (ThumbSize));
            foreach (var size in sizes.Where(size => size > 0))
            {              
                ret.Add(CreateWidthThumb(commandId, img, (int)size));
                //ret.Add(CreateLengthThumb(commandId, img, (int)size));
                ret.Add(CreateOriginalThumb(commandId, img, (int)size));
                ret.Add(CreateSquareThumb(commandId, img, (int)size));
            }
            return ret;
        }

        private ImageDimension CreateSquareThumb(string commandId, Image img, int size)
        {
            var aspectImg = EnsureAspectRatio(img, 1);
            using (var thumb = aspectImg != null ? aspectImg.Resize(size, size) : img.Resize(size, size))
            {
                var ext = ImageUtil.GetIdFileExtension(ThumbOrientation.Square, (ThumbSize)size);
                _blobStorage.SetBlob(commandId + ext, thumb.GetBytes(), BlobProperties.JpegContentTypeDefault);

                if (aspectImg != null)
                    aspectImg.Dispose();

                return GetDim(thumb, ext, ThumbOrientation.Square);
            }

        }

        private ImageDimension CreateOriginalThumb(string commandId, Image img, int size)
        {
            using (var thumb = img.Width > img.Height
                                   ? img.Resize(size, img.Height)
                                   : img.Resize(img.Width, size))
            {
                var ext = ImageUtil.GetIdFileExtension(ThumbOrientation.Original, (ThumbSize) size);
                _blobStorage.SetBlob(commandId + ext, thumb.GetBytes(), BlobProperties.JpegContentTypeDefault);
                return GetDim(thumb, ext, ThumbOrientation.Original);
            }
        }

//        private ImageDimension CreateLengthThumb(string commandId, Image img, int size)
//        {
//            using (var thumb = img.Resize((int) Math.Ceiling(img.Width*(size/(double) img.Height)), size))
//            {
//                var ext = ImageUtil.GetIdFileExtension(ThumbOrientation.Vertical, (ThumbSize)size);
//                _blobStorage.SetBlob(commandId + ext, thumb.GetBytes(), BlobProperties.JpegContentTypeDefault);
//                return GetDim(thumb, ext, ThumbOrientation.Vertical);
//            }
//        }

        private ImageDimension CreateWidthThumb(string commandId, Image img, int size)
        {
            using (var thumb = img.Resize(size, (int) Math.Ceiling(img.Height*(size/(double) img.Width))))
            {
                var ext = ImageUtil.GetIdFileExtension(ThumbOrientation.Horizontal, (ThumbSize)size);
                _blobStorage.SetBlob(commandId + ext, thumb.GetBytes(), BlobProperties.JpegContentTypeDefault);
                return GetDim(thumb, ext, ThumbOrientation.Horizontal);
            }
        }

        #endregion



        private static Image EnsureAspectRatio(Image img, double aspectRatio)
        {
            var imgcurr = img;
            var resWidth = CropWidthIfAspectOver(imgcurr, aspectRatio);
            if (resWidth != null)
                imgcurr = resWidth;

            var resHeight = CropHeightIfAspectOver(imgcurr, aspectRatio);
            if (resHeight != null)
                imgcurr = resHeight;

            if(imgcurr != img)
            {
                if(resWidth != null && resHeight != null)
                    resWidth.Dispose();
                return imgcurr;
            }

            return null;
        }

        private static Image CropWidthIfAspectOver(Image img, double widthAspectRatio)
        {
            //ensure no stupidly wide images
            if ((double) img.Width/img.Height <= widthAspectRatio) return null;
            var newWidth = img.Height * widthAspectRatio;
            return CropWidth(img, img.Width - (int)newWidth);
        }

        private static Image CropWidth(Image img, int numPixelsToRemove)
        {
            numPixelsToRemove = numPixelsToRemove / 2;
          
            return img.Crop(0, numPixelsToRemove, 0, numPixelsToRemove);
        }

        private static Image CropHeightIfAspectOver(Image img, double heightAspectRatio)
        {
            //ensure no stupidly long images
            if ((double) img.Height/img.Width <= heightAspectRatio) return null;
            var newHeight = img.Width * heightAspectRatio;
            return CropHeight(img, img.Height - (int)newHeight);
        }

        private static Image CropHeight(Image img, int numPixelsToRemove)
        {
            numPixelsToRemove = numPixelsToRemove / 2;
            return img.Crop((int)numPixelsToRemove, 0, (int)numPixelsToRemove, 0);
        }

        private static Image EnsureMaxiums(Image img)
        {
            //ensure max's aren't violated
            if (img.Width > MaxWidth || img.Height > MaxHeight)
            {
                return img.Resize(Math.Min((int)img.Width, MaxWidth), Math.Min((int)img.Height, MaxHeight));
            }

            return null;
        }
    }


}