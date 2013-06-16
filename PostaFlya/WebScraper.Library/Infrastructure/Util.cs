using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using OpenQA.Selenium;
using PostaFlya.Application.Domain.Google.Places;
using PostaFlya.Domain.Venue;
using PostaFlya.Models.Location;
using Website.Application.Google.Places.Details;

namespace WebScraper.Library.Infrastructure
{
    public static class Util
    {
        public static string RemoveLinesContaining(this string text, params string[] terms)
        {
            return text.Split('\n').Where(s => !terms.Any(term => s.ToLower().Contains(term.ToLower()) ))
                       .Aggregate(new StringBuilder(), (builder, s) => builder.Append(s + "\n"))
                       .ToString();
        }

        public static VenueInformationModel GetVenueInformationModelFromGooglePleacesRef(String googlePlacesRef)
        {
            var res = new PlaceDetailsRequest(googlePlacesRef).Request().Result;


            if (!PlaceDetailsRequest.IsFailure(res))
            {
                var venuInfo = new VenueInformation().MapFrom(res.result);
                return venuInfo.ToViewModel();
            }

            return null;
        }

        public static string ToTextWithLineBreaks(this IWebElement element, string cssSelectors = "h1,h2,p")
        {
            var elements = element.FindElements(By.CssSelector(cssSelectors));
            var descList = new List<string>();
            foreach (var iWebElement in elements)
            {
                var text = iWebElement.Text;
                if (text.SeemsOk())
                    descList.Add(text.Sanitize());
            }

            return descList.Aggregate(new StringBuilder(), (builder, s) => builder.Append(s + "\n")).ToString();
        }

        public static string HtmlEncode(this string s)
        {
            return HttpUtility.HtmlEncode(s);
        }

        public static string HtmlDecode(this string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

        private static readonly Regex Tags = new Regex("<[^>]*(>|$)",
            RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        private static readonly Regex SpecChars = new Regex("&[^; ]*;",
    RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        public static string Sanitize(this string html)
        {
            if (String.IsNullOrWhiteSpace(html)) return html;

            // match every HTML tag in the input
            var tags = Tags.Matches(html);
            for (var i = tags.Count - 1; i > -1; i--)
            {
                var tag = tags[i];
                html = html.Remove(tag.Index, tag.Length);
            }

            var specChars = SpecChars.Matches(html);
            for (var i = specChars.Count - 1; i > -1; i--)
            {
                var spec = specChars[i];
                html = html.Remove(spec.Index, spec.Length);
            }

            return html;
        }

        public static bool SeemsOk(this string html)
        {
            if (html == null) return false;

            // match every HTML tag in the input
            var tags = Tags.Matches(html);
            if (tags.Count > 0)
                return false;


            var specChars = SpecChars.Matches(html);
            if (specChars.Count > 0)
                return false;

            return true;
        }

        public static string SafeText(this string text)
        {
            return text.Sanitize();//don't worry about encode.HtmlEncode();
        }
    }
}
