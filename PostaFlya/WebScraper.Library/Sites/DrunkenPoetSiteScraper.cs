using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OpenQA.Selenium;
using PostaFlya.Application.Domain.Google.Places;
using PostaFlya.Domain.Venue;
using PostaFlya.Models.Location;
using WebScraper.Library.Infrastructure;
using WebScraper.Library.Model;
using Website.Application.Google.Places.Details;

namespace WebScraper.Library.Sites
{
    public class DrunkenPoetSiteScraper : SiteScraperInterface
    {
        protected Logger Logger = LogManager.GetCurrentClassLogger();


        private readonly IWebDriver _driver;

        public const string BaseUrl = "http://thedrunkenpoet.com.au";
        private const string Url = BaseUrl + "/whats_on";
        private const string Tags = "music";

        private readonly VenueInformationModel _venueInformationModel;

        private const string GooglePlacesId =
            "CnRuAAAAUPO86WNFIbcJ7UWvitq3AAsyZYZt5RsaVWtlXUgflkbslYP1bkRH8UIieKxAdAIfpFwvGvoCqE3FCxdUipNK7R1aJ01HE8QlRjxGweu8sSv-vTleMeZ1eVBTGqvijVpaMgnZPnXoDD5zT3S4jsnyBBIQ1v3KMz6sXnI6fVScnJ7OkBoUGwftj9CHIeOLq_LWjKqBVz10bTI";
        public DrunkenPoetSiteScraper(IWebDriver driver)
        {
            _driver = driver;

            var res = new PlaceDetailsRequest(GooglePlacesId).Request().Result;
            if (!PlaceDetailsRequest.IsFailure(res))
            {
                var venuInfo = new VenueInformation().MapFrom(res.result);
                _venueInformationModel = venuInfo.ToViewModel();    
            }
            
        }

        public string SiteName { get { return RegisterSites.Retreat; } }
        public List<ImportedFlyerScraperModel> GetFlyersFrom(DateTime eventDateStart, DateTime eventDateEnd)
        {
            _driver.Navigate().GoToUrl(Url);
            var eles = _driver.FindElements(By.CssSelector("#whats_on_widget .event"));


            var list = eles.Select(e => SecondPassGetDetails(e, eventDateStart, eventDateEnd)).ToList();

            _driver.Navigate().GoToUrl("about:blank");
            _driver.Quit();
            _driver.Dispose();
            var ret = list.Where(model => model != null).ToList();

            ret.ForEach(model =>
                {
                    model.VenueInfo = _venueInformationModel;
                    model.Tags = Tags;
                });


            return ret;
        }


        private ImportedFlyerScraperModel SecondPassGetDetails(IWebElement entry, DateTime eventDateStart, DateTime eventDateEnd)
        {
            try
            {
                var flyer = new ImportedFlyerScraperModel();
                flyer.EventDates = new List<DateTime>();
                var date = entry.FindElement(By.ClassName("date"));
                var dateText = date.Text.Trim() + DateTime.Now.Year;
                dateText = dateText.Substring(3);

                CultureInfo provider = CultureInfo.InvariantCulture;
                flyer.EventDates.Add(DateTime.ParseExact(dateText, "ddMMMyyyy", provider));
                if (flyer.EventDates.First() < eventDateStart || flyer.EventDates.First() > eventDateEnd)
                    return null;

                flyer.Title = entry.FindElement(By.CssSelector("div.title")).Text;

                //flyer.Description = entry.FindElement(By.CssSelector("span.gig-timestamp")).Text + "\n";
                flyer.Description = entry.FindElement(By.CssSelector("div.text")).ToTextWithLineBreaks();
                    

                //flyer.ImageUrl 
                flyer.Source = Url;
                var imgEle = entry.FindElement(By.CssSelector("div.text img"));
                if (imgEle != null)
                {
                    flyer.ImageUrl = imgEle.GetAttribute("src");
                }

                
                return flyer;

            }
            catch (Exception e)
            {
                Logger.ErrorException("", e);
                return null;
            }

        }
    }
}
