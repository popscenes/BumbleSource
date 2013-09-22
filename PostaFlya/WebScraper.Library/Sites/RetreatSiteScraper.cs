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
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Venue;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using WebScraper.Library.Infrastructure;
using WebScraper.Library.Model;
using Website.Application.Google.Places.Details;

namespace WebScraper.Library.Sites
{

    public class RetreatSiteScraper : SiteScraperInterface
    {
        private readonly IWebDriver _driver;

        public const string BaseUrl = "http://retreathotelbrunswick.com.au";
        private const string Url = BaseUrl + "/gigs/";
        private const string Tags = "music";

        protected Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly VenueInformationModel _venueInformationModel;

        private const string GooglePlacesId =
            "CnRrAAAAE_4ok69hgAvu10KbAKQjd7ZZ-_aliGHbj63BJLZ1aSx8pv8d98iSRHaDeevLxDH4KVmx6Eigru0wMJeRpiyUKihxO0_aVhaEEG8-lbQJgzg7rclCxZ32el6zupm2ylbgneQKQsK5yzD5h5aoHcA4rhIQr-EKUkzaedmRdfQmHhZ-eBoUBjcQXtav0KfIJ4fo1foLoGbv1cw";

        public RetreatSiteScraper(IWebDriver driver)
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
            var eles = _driver.FindElements(By.CssSelector("ul.posts li"));


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
                CultureInfo provider = CultureInfo.InvariantCulture;
                flyer.EventDates.Add(DateTime.ParseExact(date.Text.Trim(), "dd-MM-yyyy", provider));
                if (flyer.EventDates.First() < eventDateStart || flyer.EventDates.First() > eventDateEnd)
                    return null;

                flyer.Title = entry.FindElement(By.CssSelector("div.description h3")).Text;

                //flyer.Description = entry.FindElement(By.CssSelector("span.gig-timestamp")).Text + "\n";
                flyer.Description += entry.FindElement(By.CssSelector("p.entry-content")).Text;
                    

                //flyer.ImageUrl 
                flyer.Source = new Uri(entry.FindElement(By.CssSelector(".image-wrapper a")).GetAttribute("href")).ToString();
                var img = new Uri(entry.FindElement(By.CssSelector(".image-wrapper a img")).GetAttribute("src"));
                var src = System.Web.HttpUtility.ParseQueryString(img.Query)["src"];
                flyer.ImageUrl = System.Web.HttpUtility.UrlDecode(src);
                
                return flyer;

            }
            catch (Exception e)
            {
                Logger.LogException(LogLevel.Error, "retreat failed", e );
                return null;
            }

        }

    }
}
