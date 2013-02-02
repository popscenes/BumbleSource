using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Common.Util
{
    public static class UriUtil
    {
        public static string GetCoreDomain(string urlString)
        {
            var uri = new Uri(urlString);
            var parts = uri.Host.Split('.');
            if(parts.Length >= 2)
                return parts[parts.Length - 2] + "." + parts[parts.Length - 1];
            return uri.Host;
        }

        public static bool IsWwwSubDomain(this Uri source)
        {
            return source.Host.ToLower().Contains("www.");
        }

        public static Uri RemoveWww(this Uri source)
        {
            var parts = source.Host.ToLower().Split('.');
            var builder = new UriBuilder(source) {Host = ""};
            foreach (var part in parts.Where(p => !p.Equals("www")))
            {
                if (builder.Host.Length > 0)
                    builder.Host += ".";
                builder.Host += part;
            }
            return builder.Uri;
        }
    }
}
