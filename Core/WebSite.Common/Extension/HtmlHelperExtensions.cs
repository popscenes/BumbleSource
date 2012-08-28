using System.Text;
using System.Web.Mvc;
using System.Web.Optimization;

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
    }
}