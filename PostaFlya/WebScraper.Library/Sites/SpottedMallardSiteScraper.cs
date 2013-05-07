using System;
using System.Collections.Generic;
using System.Linq;
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

        public SpottedMallardSiteScraper(IWebDriver driver)
        {
            _driver = driver;
        }

        public string SiteName { get { return RegisterSites.SpottedMallard; } }
        public List<ImportedFlyerScraperModel> GetFlyersFrom(DateTime eventDateStart)
        {
            _driver.Navigate().GoToUrl(Url);

            var eles = _driver.FindElements(By.ClassName("blogSummary"));

            var list = (from webEle in eles let ele = webEle.FindElement(By.ClassName("readmore"))
                        select ele.GetAttribute("href")).ToList();

            foreach (var url in list)
            {
                _driver.Navigate().GoToUrl(url);
            }

            _driver.Navigate().GoToUrl("about:blank");
            _driver.Dispose();
            return new List<ImportedFlyerScraperModel>();

        }
    }
}