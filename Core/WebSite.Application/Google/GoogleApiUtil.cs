using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Configuration;

namespace Website.Application.Google
{
    public static class GoogleApiUtil
    {
        public const string PlacesKey = "GooglePlacesApiKey";
        public const string MapsKey = "GoogleMapsApiKey";

        public static string AddApiKey(this string urlSource, string keySource = MapsKey)
        {
            var key = "";
            if (Config.Instance != null && (key = Config.Instance.GetSetting(keySource)) != null)
            {
                return urlSource + "&key=" + key;
            }
            return urlSource;

        }
    }
}
