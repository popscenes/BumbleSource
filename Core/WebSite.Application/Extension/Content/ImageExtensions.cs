using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Website.Application.Extension.Content
{
    public static class ImageExtensions
    {
        public static Image Crop(this Image img, int top, int left, int bottom, int right)
        {
            var cropRect = new Rectangle(left, top, img.Width - left - right, img.Height - top - bottom);
            return img.CropImage(cropRect);
        }
        public static Image CropImage(this Image img, Rectangle cropArea)
        {
            if (cropArea.Width > img.Width)
                cropArea.Width = img.Width;
            if (cropArea.Height > img.Height)
                cropArea.Height = img.Height;

            using (var bmpImage = new Bitmap(img))
            {
                var bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
                return (Image) (bmpCrop);
            }
        }

        public static Image Resize(this Image img, int width, int height)
        {
            return img.ResizeImage(new Size(width, height));
        }

        public static Image ResizeImage(this Image img, Size size)
        {
            int sourceWidth = img.Width;
            int sourceHeight = img.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)Math.Round(sourceWidth * nPercent);
            int destHeight = (int)Math.Round(sourceHeight * nPercent);

            Bitmap b = null;
            Graphics g = null;

            try
            {
                b = new Bitmap(destWidth, destHeight);
                g = Graphics.FromImage((Image)b);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, destWidth, destHeight);
                g.Dispose();

//                return img.GetThumbnailImage(destWidth, destHeight, null, IntPtr.Zero);
            }
            catch (Exception e)
            {
                Trace.TraceError("ResizeImage Error: {0}, Stack {1}", e.Message, e.StackTrace);
                if(b != null)
                    b.Dispose();
                if(g != null)
                    g.Dispose();
                throw;
            }

            return (Image)b;
//            return null;
        }

        public static byte[] GetBytes(this Image img, ImageFormat fmt)
        {   
            using (var savems = new MemoryStream())
            {
                img.Save(savems, fmt);
                return savems.ToArray();
            }
        }

        public static byte[] GetBytes(this Image img)
        {
            return GetBytes(img, ImageFormat.Jpeg);
        }
    }
}