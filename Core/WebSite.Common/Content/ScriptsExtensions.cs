using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Website.Infrastructure.Configuration;

namespace Website.Common.Content
{
    public static class ScriptsExtensions
    {
        private static readonly Regex _regexSrc = new Regex(@"<script.*src=""([^""]*)"".*>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        public static IEnumerable<string> GetScriptSrc(this IHtmlString tag)
        {
            var scriptTag = tag.ToHtmlString();
            var scriptsrc = _regexSrc.Matches(scriptTag);
            for (int i = 0; i < scriptsrc.Count; i++)
            {
                var match = scriptsrc[i];
                var src = match.Groups[1].Captures[0].Value;
                yield return (src.StartsWith("/")) ? Config.Instance.GetSetting("SiteUrl") + src : src; 
            }

        }

        private static readonly Regex _regexHref = new Regex(@"<link.*href=""([^""]*)"".*>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        public static IEnumerable<string> GetHref(this IHtmlString tag)
        {
            var linkTag = tag.ToHtmlString();
            var hrefs = _regexHref.Matches(linkTag);
            for (int i = 0; i < hrefs.Count; i++)
            {
                var match = hrefs[i];
                var src = match.Groups[1].Value;
                yield return (src.StartsWith("/")) ? Config.Instance.GetSetting("SiteUrl") + src : src;
            }
        }

        public static string ToJsArray(this IEnumerable<string> array)
        {
            return array.Aggregate(new StringBuilder("["), (builder, s) =>
                {
                    if (builder.Length > 1)
                        builder.Append(",");
                    builder.Append('\'');
                    builder.Append(s.Replace("\n", @"\n").Replace("'", @"\'"));
                    builder.Append('\'');
                    return builder;
                }) + "]";
        }
    }
}
