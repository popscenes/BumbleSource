using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Website.Application.Content;
using Website.Domain.Location;

namespace Website.Application.Domain.Location.Web
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString LocationImage(this HtmlHelper htmlHelper, Website.Domain.Location.Location location,
            int width = 400, int height = 400, int zoom = 16, object htmlAttributes = null)
        {
            var url = GoogleMaps.MapUrl(location.Latitude, location.Longitude
                , new List<GoogleMaps.Marker>()
                    {
                        new GoogleMaps.Marker()
                            {
                                Latitude = location.Latitude, 
                                Longitude = location.Longitude                          
                            }
                    }
                , width, height, zoom);

            var tag = new TagBuilder("img");
            if (htmlAttributes != null)
                tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tag.Attributes.Add("src", url);
            tag.Attributes.Add("width", width.ToString(CultureInfo.InvariantCulture));
            tag.Attributes.Add("height", height.ToString(CultureInfo.InvariantCulture));

            return new MvcHtmlString(tag.ToString(TagRenderMode.SelfClosing));
        }

        public static string LocationMapSearchUrl(this HtmlHelper htmlHelper, Website.Domain.Location.Location location)
        {
            return GoogleMaps.MapSearchLink(location.GetAddressDescription());
        }
    }
}
