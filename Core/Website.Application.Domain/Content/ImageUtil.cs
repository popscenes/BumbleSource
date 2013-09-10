using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Website.Domain.Content;

namespace Website.Application.Domain.Content
{
    public static class ImageUtil
    {
        public const double A4AspectRatio = 1.414213562373095;//not used atm
        public static readonly Size A4300DpiSize = new Size(2480, 3508);

        public static string GetIdFileExtension(ThumbOrientation orientation, ThumbSize thumbSize, String extension)
        {
            var ret = new string(Char.ToLower(orientation.ToString()[0]), 1);
            ret += (int)thumbSize;

            return ret + "." + extension;
        }
        public static string GetIdFileExtension(ThumbOrientation orientation, ThumbSize thumbSize)
        {
            return GetIdFileExtension(orientation, thumbSize, "jpg");
        }

        public static ImageFormat GetSaveFormat(string extension)
        {
            if (String.Equals(extension, "png", StringComparison.CurrentCultureIgnoreCase))
            {
                return ImageFormat.Png;
            }
            if (String.Equals(extension, "gif", StringComparison.CurrentCultureIgnoreCase))
            {
                return ImageFormat.Gif;
            }
            if (String.Equals(extension, "bmp", StringComparison.CurrentCultureIgnoreCase))
            {
                return ImageFormat.Bmp;
            }
            if (String.Equals(extension, "tiff", StringComparison.CurrentCultureIgnoreCase))
            {
                return ImageFormat.Tiff;
            }
            return ImageFormat.Jpeg;
        }

        public static string GetIdFileExtension(bool keepOriginalFileType, String extension)
        {
             return keepOriginalFileType ? "." + extension : ".jpg";
        }
        
        public static string GetIdFileExtension()
        {
            return ".jpg";
        }

        public static string GetThumbUrlForImage(this Uri imageUri, ThumbOrientation orientation = ThumbOrientation.Horizontal, ThumbSize thumbSize = ThumbSize.S300)
        {
            if (imageUri == null)
                return null;

            return GetThumbUrlForImageUri(imageUri.ToString(), orientation, thumbSize);
        }

        public static string GetThumbUrlForImageUri(string imageUri, ThumbOrientation orientation = ThumbOrientation.Horizontal, ThumbSize thumbSize = ThumbSize.S300)
        {
            if (imageUri == null)
                return null;

            var index = imageUri.LastIndexOf('.');
            if (index >= 0)
                imageUri = imageUri.Remove(index);

            return imageUri + GetIdFileExtension(orientation, thumbSize);
        }
    }

    public enum ThumbSize
    {
        S300 = 300,
        S400 = 400,
        S900 = 900,
    }


}
