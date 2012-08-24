using System;
using System.Diagnostics;
using System.IO;
using WebSite.Application.Binding;
using WebSite.Application.Content;
using WebSite.Application.Extension.Content;
using WebSite.Infrastructure.Command;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Image = System.Drawing.Image;

namespace Website.Application.Domain.Content.Command
{
    public class ImageProcessCommandHandler : CommandHandlerInterface<ImageProcessCommand>
    {
        public const int MaxWidthHeight = 750;
        public const double A4AspectRatio = 1.414213562373095;//not used atm


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

                var convdata = curr.GetBytes();
                _blobStorage.SetBlob(command.CommandId, convdata);

                CreateThumbs(command.CommandId, curr);

                ProcessMetaData(img, command.CommandId);

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
            catch (Exception e)
            {
                if (aspectImg != null)
                    aspectImg.Dispose();
                if (maxDimImg != null)
                    maxDimImg.Dispose();
                throw;
            }
        }

        private void ProcessMetaData(Image img, string imgId)
        {
            var loc = new Website.Domain.Location.Location();
            var exif = new ExifImage(img);

            var hasMetaData = false;
            var latitude = exif.GetLatitude();
            var longitude = exif.GetLongitude();
            if (latitude.HasValue && longitude.HasValue)
            {
                loc.Latitude = latitude.Value;
                loc.Longitude = longitude.Value;
                hasMetaData = true;
            }

            var title = exif.GetImageTitle();
            if (string.IsNullOrWhiteSpace(title))
                title = exif.GetImageDescription();
            if(!string.IsNullOrWhiteSpace(title))
                hasMetaData = true;

            if (!hasMetaData) return;

            _commandBus.Send(new SetImageMetaDataCommand()
            {
                Id = imgId,
                Location = loc,
                Title = title
            });
        }

        private void CreateThumbs(string commandId, Image img)
        {
            foreach (ThumbSize size in Enum.GetValues(typeof(ThumbSize)))
            {
                CreateWidthThumb(commandId, img, (int)size);
                CreateLengthThumb(commandId, img, (int)size);
                CreateOriginalThumb(commandId, img, (int)size);
                CreateSquareThumb(commandId, img, (int)size);
            }
        }

        private void CreateSquareThumb(string commandId, Image img, int size)
        {
            var idExtension = ImageUtil.GetIdExtensionForThumb(ThumbOrientation.Square, (ThumbSize)size);
            var aspectImg = EnsureAspectRatio(img, 1);
            var thumb = aspectImg != null ? aspectImg.Resize(size, size) : img.Resize(size, size);
            _blobStorage.SetBlob(commandId + idExtension, thumb.GetBytes());

            if (aspectImg != null)
                aspectImg.Dispose();
            thumb.Dispose();
        }

        private void CreateOriginalThumb(string commandId, Image img, int size)
        {
            var idExtension = ImageUtil.GetIdExtensionForThumb(ThumbOrientation.Original, (ThumbSize)size);

            var thumb = img.Width > img.Height ? 
                            img.Resize(size, img.Height) : 
                            img.Resize(img.Width, size);

            _blobStorage.SetBlob(commandId + idExtension, thumb.GetBytes());                
            
            thumb.Dispose();
        }

        private void CreateLengthThumb(string commandId, Image img, int size)
        {
            var idExtension = ImageUtil.GetIdExtensionForThumb(ThumbOrientation.Vertical, (ThumbSize)size);
            var thumb = img.Resize((int)Math.Ceiling(img.Width * (size / (double)img.Height)), size);
            _blobStorage.SetBlob(commandId + idExtension, thumb.GetBytes());
            thumb.Dispose();
        }

        private void CreateWidthThumb(string commandId, Image img, int size)
        {
            var idExtension = ImageUtil.GetIdExtensionForThumb(ThumbOrientation.Horizontal, (ThumbSize) size);
            var thumb = img.Resize(size, (int)Math.Ceiling(img.Height * (size / (double)img.Width)));
            _blobStorage.SetBlob(commandId + idExtension, thumb.GetBytes());   
            thumb.Dispose();
        }

        #endregion

        private Image EnsureAspectRatio(Image img, double aspectRatio)
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
            if (img.Width > MaxWidthHeight || img.Height > MaxWidthHeight)
            {
                return img.Resize(Math.Min((int) img.Width, MaxWidthHeight), Math.Min((int) img.Height, MaxWidthHeight));
            }

            return null;
        }
    }


}