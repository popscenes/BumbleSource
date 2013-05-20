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
        private static readonly Regex _regex = new Regex(@".*src""(.*)"".*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string GetScriptSrc(this IHtmlString tag)
        {
            var scriptTag = tag.ToHtmlString();
            var scriptsrc = _regex.Matches(scriptTag);
            if (scriptsrc.Count == 0)
                return null;
            var match = scriptsrc[0];
            var src = match.Groups[0].Value;
            return (src.StartsWith("/")) ? Config.Instance.GetSetting("SiteUrl") + src : src;
        }
    }
}
