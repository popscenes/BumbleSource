using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Website.Common.Extension
{
    public static class StringExtensions
    {
        public static string HtmlEncode(this string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

        private static readonly Regex Tags = new Regex("<[^>]*(>|$)",
            RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        public static string Sanitize(this string html)
        {
            if (String.IsNullOrEmpty(html)) return html;

            // match every HTML tag in the input
            var tags = Tags.Matches(html);
            for (var i = tags.Count - 1; i > -1; i--)
            {
                var tag = tags[i];
                html = html.Remove(tag.Index, tag.Length);
            }

            return html;
        }

        public static string SafeText(this string text)
        {
            return text.Sanitize();//don't worry about encode.HtmlEncode();
        }

        public static string GetEmptyIfNull(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            return text;
        }
    }
}