using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using WebScraper.Library.Infrastructure;
using WebScraper.Library.Model;

namespace WebScraper.Library.Sites
{

//    public class RetreatSiteScraper : SiteScraperInterface
//    {
//        private readonly IWebDriver _driver;
//
//        private const string Url = "http://retreathotelbrunswick.com.au/";
//
//        public RetreatSiteScraper(IWebDriver driver)
//        {
//            _driver = driver;
//        }
//
//        public string SiteName { get { return RegisterSites.Retreat; } }
//        public List<ImportedFlyerScraperModel> GetFlyersFrom(DateTime eventDateStart)
//        {
//            _driver.Navigate().GoToUrl(Url);
//
//            _driver.Close();
//            
//            return new List<ImportedFlyerScraperModel>();
//
//        }
//    }
}
