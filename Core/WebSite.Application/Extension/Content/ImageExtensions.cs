using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
            return img.ResizeImageMaintainAspect(new Size(width, height));
        }

        public static Image ResizeByWidth(this Image img, int width)
        {
            return img.ResizeImageMaintainAspect(new Size(width, -1));
        }

        public static Image ResizeByHeight(this Image img, int height)
        {
            return img.ResizeImageMaintainAspect(new Size(-1, height));
        }

        public static Image ResizeImageMaintainAspect(this Image img, Size size)
        {
            int sourceWidth = img.Width;
            int sourceHeight = img.Height;

            decimal mult = 0;
            decimal multW = 0;
            decimal multH = 0;

            multW = size.Width >= 0 ? (size.Width / (decimal)sourceWidth) : -1;
            multH = size.Height >= 0 ? (size.Height / (decimal)sourceHeight) : -1;

            if (multW == -1)
                mult = multH;
            else if (multH == -1)
                mult = multW;
            else if (multH < multW)
                mult = multH;
            else
                mult = multW;

            int destWidth = (int)Math.Round(sourceWidth * mult);
            int destHeight = (int)Math.Round(sourceHeight * mult);

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

        public static void CopyProperties(this Image target, Image src)
        {
            foreach (var id in src.PropertyIdList)
            {
                var propsrc = src.GetPropertyItem(id);

                target.UpdatePropertyItem(id, propsrc.Type, propsrc.Value.ToArray());
            }

        }

        public static void UpdatePropertyItem(this Image image, int id, short type, byte[] data)
        {
            PropertyItem prop = null;
            if (image.PropertyIdList.Contains(id))
                prop = image.GetPropertyItem(id);
            if (prop == null)
                prop = (PropertyItem)Activator.CreateInstance(typeof(PropertyItem), true);

            prop.Id = id;
            prop.Type = type;
            prop.Len = data.Length;
            prop.Value = data;
            image.SetPropertyItem(prop);
        }

        public static byte[] GetBytes(this Image img, ImageFormat fmt)
        {   
            using (var savems = new MemoryStream())
            {
                using (var tempImage = new Bitmap(img))
                {
                    tempImage.CopyProperties(img);
                    tempImage.Save(savems, fmt);
                }

                return savems.ToArray();
            }
        }

        public static byte[] GetBytes(this Image img)
        {
            return GetBytes(img, ImageFormat.Jpeg);
        }

        public static Image GetImage(this byte[] imgData)
        {
            using (var ms = new MemoryStream(imgData))
            {
                return Image.FromStream(ms);
            } 
        }

        public static Image ImageFromDataUri(this string data)
        {
            var base64Data = Regex.Match(data, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            if (string.IsNullOrWhiteSpace(base64Data))
                return null;

            var binData = Convert.FromBase64String(base64Data);
            return binData.GetImage();

        }

        public static Stream ImageStreamFromDataUri(this string data)
        {
            if (data == null) return null;

            var base64Data = Regex.Match(data, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            if (string.IsNullOrWhiteSpace(base64Data))
                return null;

            var binData = Convert.FromBase64String(base64Data);
            return new MemoryStream(binData);
        }
    }
}