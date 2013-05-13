using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Infrastructure.Configuration;

namespace Website.Application.Google.Content
{
    public static class GoogleMaps
    {

        public class Marker
        {
            public Marker()
            {
                Colour = "orange";
                Label = "A";
            }
            public string Label { get; set; }
            public double Longitude { get; set; }
            public double Latitude { get; set; }
            public string Colour { get; set; }
        }



        public static String MapUrl(double centreLat, double centreLong, IEnumerable<Marker> markers,
            int width = 400, int height = 400, int zoom = 16)
        {
            var ret = String.Format("http://maps.googleapis.com/maps/api/staticmap?sensor=false&center={0},{1}&size={2}x{3}&zoom={4}",
                centreLat, centreLong,
                width, height, zoom           
                );
            ret = markers
                .Select(marker => HttpUtility.UrlEncode(String.Format("color:{0}|label:{1}|{2},{3}", marker.Colour, marker.Label, marker.Latitude, marker.Longitude)))
                .Aggregate(ret, (current, param) => current + ("&markers=" + param));
            return ret.AddApiKey();
        }

        public static string MapSearchLink(string searchAddress)
        {
            return String.Format("http://maps.google.com/?q={0}", HttpUtility.UrlEncode(searchAddress));
        }
    }
}
