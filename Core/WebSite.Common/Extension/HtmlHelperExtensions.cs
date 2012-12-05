using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Website.Common.Extension
{
    public static class HtmlHelperExtensions
    {
        //hopefully gone when budling doesn't compress by default in debug
        public static MvcHtmlString DebugBundleJs(this HtmlHelper helper, string bundlePath)
        {
            var jsTag = new TagBuilder("script");
            jsTag.MergeAttribute("type", "text/javascript");

            return DebugBundle(helper, bundlePath, jsTag, "src");
        }

        public static MvcHtmlString DebugBundleCss(this HtmlHelper helper, string bundlePath)
        {
            var cssTag = new TagBuilder("link");
            cssTag.MergeAttribute("rel", "stylesheet");
            return DebugBundle(helper, bundlePath, cssTag, "href");
        }

        private static MvcHtmlString DebugBundle(this HtmlHelper helper, string bundlePath, TagBuilder baseTag, string srcAtt)
        {
            var httpContext = helper.ViewContext.HttpContext;
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);

            Bundle bundle = BundleTable.Bundles.GetBundleFor(bundlePath);
            var htmlString = new StringBuilder();

            if (bundle != null)
            {
                var bundleContext = new BundleContext(helper.ViewContext.HttpContext, BundleTable.Bundles, urlHelper.Content(bundlePath));


                foreach (var file in bundle.EnumerateFiles(bundleContext))
                {
                    var basePath = httpContext.Server.MapPath("~/");
                    if (file.FullName.StartsWith(basePath))
                    {
                        var relPath = urlHelper.Content("~/" + file.FullName.Substring(basePath.Length));
                        baseTag.MergeAttribute(srcAtt, relPath, true);
                        htmlString.AppendLine(baseTag.ToString());
                    }
                }

            }

            return new MvcHtmlString(htmlString.ToString());
        }


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
        public static MvcHtmlString QrCode(this HtmlHelper htmlHelper, string data, int size = 80, int margin = 4, QrCodeErrorCorrectionLevel errorCorrectionLevel = QrCodeErrorCorrectionLevel.Low, object htmlAttributes = null)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (size < 1)
                throw new ArgumentOutOfRangeException("size", size, "Must be greater than zero.");
            if (margin < 0)
                throw new ArgumentOutOfRangeException("margin", margin, "Must be greater than or equal to zero.");
            if (!Enum.IsDefined(typeof(QrCodeErrorCorrectionLevel), errorCorrectionLevel))
                throw new InvalidEnumArgumentException("errorCorrectionLevel", (int)errorCorrectionLevel, typeof(QrCodeErrorCorrectionLevel));

            var url = String.Format("http://chart.apis.google.com/chart?cht=qr&chld={2}|{3}&chs={0}x{0}&chl={1}", size, HttpUtility.UrlEncode(data), errorCorrectionLevel.ToString()[0], margin);

            var tag = new TagBuilder("img");
            if (htmlAttributes != null)
                tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tag.Attributes.Add("src", url);
            tag.Attributes.Add("width", size.ToString(CultureInfo.InvariantCulture));
            tag.Attributes.Add("height", size.ToString(CultureInfo.InvariantCulture));

            return new MvcHtmlString(tag.ToString(TagRenderMode.SelfClosing));
        }

        public enum QrCodeErrorCorrectionLevel
        {
            /// <summary>Recovers from up to 7% erroneous data.</summary>
            Low,
            /// <summary>Recovers from up to 15% erroneous data.</summary>
            Medium,
            /// <summary>Recovers from up to 25% erroneous data.</summary>
            QuiteGood,
            /// <summary>Recovers from up to 30% erroneous data.</summary>
            High
        }
    }
}