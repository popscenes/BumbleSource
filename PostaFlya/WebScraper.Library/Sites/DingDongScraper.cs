using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using PostaFlya.Application.Domain.Google.Places;
using PostaFlya.Domain.Venue;
using PostaFlya.Models.Location;
using WebScraper.Library.Infrastructure;
using WebScraper.Library.Model;
using Website.Application.Google.Places.Details;

namespace WebScraper.Library.Sites
{
    class DingDongScraper: SiteScraperInterface
    {
        public string SiteName { get { return RegisterSites.DingDong; } }
        public List<ImportedFlyerScraperModel> GetFlyersFrom(DateTime eventDateStart, DateTime eventDateEnd)
        {
            _driver.Navigate().GoToUrl(Url);
            var eles = _driver.FindElements(By.CssSelector("div.gig"));
            var list = eles.Select(e => FirstPassGetSummary(e, eventDateStart, eventDateEnd)).ToList();

            var ret = list.Where(model => model != null).ToList();

            foreach (var flyer in list.Where(model => model != null))
            {
                SecondPassGetDetails(flyer);
            }

            _driver.Navigate().GoToUrl("about:blank");
            _driver.Quit();
            _driver.Dispose();

            ret.ForEach(model =>
            {
                model.VenueInfo = _venueInformationModel;
                model.Tags = Tags;
            });


            return ret;
        }

        private ImportedFlyerScraperModel FirstPassGetSummary(IWebElement entry, DateTime eventDateStart,
                                                               DateTime eventDateEnd)
        {
            try
            {
                var flyer = new ImportedFlyerScraperModel();
                flyer.EventDates = new List<DateTime>();
                var date = entry.FindElement(By.CssSelector(".giginfo h4"));
                var dateText = date.Text.Replace("1st", "1").Replace("2nd", "2").Replace("3rd", "3").Replace("th", "").Trim() + " " + DateTime.Now.Year;


                CultureInfo provider = CultureInfo.InvariantCulture;
                flyer.EventDates.Add(DateTime.ParseExact(dateText, "dddd d MMMM yyyy", provider));
                if (flyer.EventDates.First() < eventDateStart || flyer.EventDates.First() > eventDateEnd)
                    return null;

                flyer.Title = entry.FindElement(By.CssSelector(".giginfo h3")).Text;

                flyer.SourceDetailPage = new Uri(entry.FindElement(By.CssSelector(".giglink")).GetAttribute("href"));

                return flyer;


            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }

        }

        private void SecondPassGetDetails(ImportedFlyerScraperModel flyer)
        {
            try
            {
                _driver.Navigate().GoToUrl(flyer.SourceDetailPage.ToString());
                flyer.Description = _driver.FindElement(By.CssSelector("#singlegigright")).ToTextWithLineBreaks().RemoveLinesContaining("Gracebook"); ;
                flyer.ImageUrl = _driver.FindElement(By.CssSelector("#singlegigleft img")).GetAttribute("src").Replace("-212x300", "");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

        }



        public const string BaseUrl = "http://www.dingdonglounge.com.au/";
        private const string Url = BaseUrl;
        private const string Tags = "music";

        private readonly VenueInformationModel _venueInformationModel;

        public const string GooglePLacesRef = "CnRuAAAA9ltxLbbNrU2f-4BLznc5xhbvPRqGw8cmF27EmaIXR0o3sS5g9r7TBcqjUeQBiJpkTvBvv87k5N-5CeBEpbBez8YHEtizkSERqPR0bbgVys7FUWqGtq0AplEF886DJy0DNcdsYZDRzeN2JeFh0YEruxIQxrlZraVTnqWigL1Nkm6j-RoUXEuNzLBtkoJlwIXTau1RYxPgv-c";

        private readonly IWebDriver _driver;

        public DingDongScraper(IWebDriver driver)
        {
            _driver = driver;

            var res = new PlaceDetailsRequest(GooglePLacesRef).Request().Result;
            if (!PlaceDetailsRequest.IsFailure(res))
            {
                var venuInfo = new VenueInformation().MapFrom(res.result);
                _venueInformationModel = venuInfo.ToViewModel();
            }

        }
    }
}
