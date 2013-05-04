using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Website.Application.Content;

namespace Website.Application.Extension.Web
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Produces the markup for an image element that displays a QR Code image, as provided by Google's chart API.
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="data">The data to be encoded, as a string.</param>
        /// <param name="size">The square length of the resulting image, in pixels.</param>
        /// <param name="margin">The width of the border that surrounds the image, measured in rows (not pixels).</param>
        /// <param name="errorCorrectionLevel">The amount of error correction to build into the image.  Higher error correction comes at the expense of reduced space for data.</param>
        /// <param name="htmlAttributes">Optional HTML attributes to include on the image element.</param>
        /// <returns></returns>
        public static MvcHtmlString QrCode(this HtmlHelper htmlHelper, string data, int size = 80, int margin = 4, object htmlAttributes = null)
        {           
            var url = GoogleQrCode.QrCodeUrl(data, size, margin, QrCodeErrorCorrectionLevel.Low);

            var tag = new TagBuilder("img");
            if (htmlAttributes != null)
                tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tag.Attributes.Add("src", url);
            tag.Attributes.Add("width", size.ToString(CultureInfo.InvariantCulture));
            tag.Attributes.Add("height", size.ToString(CultureInfo.InvariantCulture));

            return new MvcHtmlString(tag.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString MapImage(this HtmlHelper htmlHelper,
            int centreLat, int centreLong, IEnumerable<GoogleMaps.Marker> markers,
            int width = 400, int height = 400, int zoom = 16, object htmlAttributes = null)
        {
            var url = GoogleMaps.MapUrl(centreLat, centreLong, markers, width, height, zoom);

            var tag = new TagBuilder("img");
            if (htmlAttributes != null)
                tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tag.Attributes.Add("src", url);
            tag.Attributes.Add("width", width.ToString(CultureInfo.InvariantCulture));
            tag.Attributes.Add("height", height.ToString(CultureInfo.InvariantCulture));

            return new MvcHtmlString(tag.ToString(TagRenderMode.SelfClosing));
        }

        
    }
}
