using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using WebScraper.Library.Infrastructure;
using WebScraper.Library.Model;

namespace WebScraper.Library.Sites
{
    public class SpottedMallardSiteScraper : SiteScraperInterface
    {
        private readonly IWebDriver _driver;

        private const string BaseUrl = "http://spottedmallard.com";
        private const string Url = BaseUrl + "/events";
        private const string Page = Url + "/?start=";

        public SpottedMallardSiteScraper(IWebDriver driver)
        {
            _driver = driver;
        }

        public string SiteName { get { return RegisterSites.SpottedMallard; } }
        public List<ImportedFlyerScraperModel> GetFlyersFrom(DateTime eventDateStart, DateTime eventDateEnd)
        {

            var list = new List<ImportedFlyerScraperModel>();
            do
            {
                _driver.Navigate().GoToUrl(Page + list.Count);
                var eles = _driver.FindElements(By.ClassName("blogSummary"));
                if (eles.Count == 0) break;
                list.AddRange(eles.Select(FirstPassSummary));

            } while (list.All(model => model.EventDates.All(time => time < eventDateEnd)));


            foreach (var flyer in list.Where(model => model != null))
            {
                SecondPassGetDetails(flyer);
            }

            _driver.Navigate().GoToUrl("about:blank");
            _driver.Dispose();
            return list.Where(model => model != null).ToList();

        }

        private void SecondPassGetDetails(ImportedFlyerScraperModel flyer)
        {
            try
            {
                _driver.Navigate().GoToUrl(flyer.SourceDetailPage.ToString());
                var entry = _driver.FindElement(By.ClassName("blogEntry"));
                flyer.Title = entry.FindElement(By.ClassName("postTitle")).Text;

                flyer.Description = entry.ToTextWithLineBreaks().
                    RemoveLinesContaining("Posted by", "Tags:");

                flyer.ImageUrl = entry.FindElement(By.CssSelector("a.image")).GetAttribute("href");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

        }

        private ImportedFlyerScraperModel FirstPassSummary(IWebElement summary)
        {
            try
            {
                var ret = new ImportedFlyerScraperModel();

                ret.SourceDetailPage = new Uri(summary.FindElement(By.ClassName("readmore")).GetAttribute("href"));
                ret.EventDates = new List<DateTime>();

                var title = summary.FindElement(By.ClassName("postTitleEvent")).Text;
                title = title.Trim();
                var date = title.Split()[0];
                DateTime result;
                CultureInfo provider = CultureInfo.InvariantCulture;
                ret.EventDates.Add(DateTime.ParseExact(date, "dd/MM/yyyy", provider));

                return ret;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }
        }
    }
}