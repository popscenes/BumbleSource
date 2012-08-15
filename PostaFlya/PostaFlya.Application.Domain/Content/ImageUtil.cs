using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostaFlya.Application.Domain.Content
{
    public static class ImageUtil
    {
        public static string GetIdExtensionForThumb(ThumbOrientation orientation, ThumbSize thumbSize)
        {
            var ret = new string(Char.ToLower(orientation.ToString()[0]), 1);
            ret += (int)thumbSize;
            return ret;
        }

        public static string GetUrlForImage(this Uri imageUri, ThumbOrientation orientation = ThumbOrientation.Vertical, ThumbSize thumbSize = ThumbSize.S250)
        {
            if (imageUri == null)
                return null;

            return GetUrlForImageUri(imageUri.ToString(), orientation, thumbSize);
        }

        public static string GetUrlForImageUri(string imageUri, ThumbOrientation orientation = ThumbOrientation.Vertical, ThumbSize thumbSize = ThumbSize.S250)
        {
            if (imageUri == null)
                return null;

            return imageUri + GetIdExtensionForThumb(orientation, thumbSize);
        }
    }
    public enum ThumbSize
    {
        S50 = 50,
        S100 = 100,
        S200 = 200,
        S250 = 250,
        S450 = 450
    }

    public enum ThumbOrientation
    {
        Horizontal,
        Vertical,
        Original,
        Square
    }
}
