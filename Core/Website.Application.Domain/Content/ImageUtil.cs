using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Website.Application.Domain.Content
{
    public static class ImageUtil
    {
        public const double A4AspectRatio = 1.414213562373095;//not used atm
        public static readonly Size A4300DpiSize = new Size(2480, 3508);

        public static string GetIdFileExtension(ThumbOrientation orientation, ThumbSize thumbSize)
        {
            var ret = new string(Char.ToLower(orientation.ToString()[0]), 1);
            ret += (int)thumbSize;
            return ret + ".jpg";
        }

        public static string GetIdFileExtension()
        {
            return ".jpg";
        }

        public static string GetThumbUrlForImage(this Uri imageUri, ThumbOrientation orientation = ThumbOrientation.Horizontal, ThumbSize thumbSize = ThumbSize.S228)
        {
            if (imageUri == null)
                return null;

            return GetThumbUrlForImageUri(imageUri.ToString(), orientation, thumbSize);
        }

        public static string GetThumbUrlForImageUri(string imageUri, ThumbOrientation orientation = ThumbOrientation.Horizontal, ThumbSize thumbSize = ThumbSize.S228)
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
        S57 = 57,
        S114 = 114,
        S228 = 228,
        S456 = 456
    }

    public enum ThumbOrientation
    {
        Horizontal,
        Vertical,
        Original,
        Square
    }
}
