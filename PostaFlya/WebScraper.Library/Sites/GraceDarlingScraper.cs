using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    internal class GraceDarlingScraper : SiteScraperInterface
    {
        public string SiteName
        {
            get { return RegisterSites.GraceDarling; }
        }

        public const string BaseUrl = "http://thegracedarlinghotel.com.au/";
        private const string Url = BaseUrl + "/gig-guide";
        private const string Tags = "music";

        private readonly VenueInformationModel _venueInformationModel;

        public const string GooglePLacesRef =
            "CoQBdQAAANUKYoSAHTyF9vHKuj89qvWfKyc5QULFU9PXY-XfoOTsqtRlnuB6sR0RPbVoMS27Y4mqAZzVCr10gVc_fKt5FBPqzl4FQIODQG5Yl-jMeM9O5UbXbJXNrd8Osz6pSaJvo7f1Abxkczk60-jhUSEI36SgirSO_YLQHEAzS1SKMgleEhBR8UZdTFFze05OoliqF1n5GhTkqWgYUoQkm7mYuqN-FaULIloFvQ";

        private readonly IWebDriver _driver;

        public GraceDarlingScraper(IWebDriver driver)
        {
            _driver = driver;

            var res = new PlaceDetailsRequest(GooglePLacesRef).Request().Result;
            if (!PlaceDetailsRequest.IsFailure(res))
            {
                var venuInfo = new VenueInformation().MapFrom(res.result);
                _venueInformationModel = venuInfo.ToViewModel();
            }

        }

        public List<ImportedFlyerScraperModel> GetFlyersFrom(DateTime eventDateStart, DateTime eventDateEnd)
        {
            _driver.Navigate().GoToUrl(Url);
            var eles = _driver.FindElements(By.CssSelector("#content .grid-item .gig.tile"));


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

        private void SecondPassGetDetails(ImportedFlyerScraperModel flyer)
        {
            try
            {
                _driver.Navigate().GoToUrl(flyer.SourceDetailPage.ToString());
                flyer.Title = _driver.FindElement(By.ClassName("page-title")).Text;

                flyer.Description = _driver.FindElement(By.ClassName("gig-description")).ToTextWithLineBreaks().RemoveLinesContaining("Gracebook"); ;

                flyer.ImageUrl = _driver.FindElement(By.CssSelector(".imagecache-gig-image")).GetAttribute("src");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }

        }


        private ImportedFlyerScraperModel FirstPassGetSummary(IWebElement entry, DateTime eventDateStart,
                                                               DateTime eventDateEnd)
        {
            try
            {
                var flyer = new ImportedFlyerScraperModel();
                flyer.EventDates = new List<DateTime>();
                var date = entry.FindElement(By.ClassName("date-display-single"));
                var dateText = date.Text.Replace("st", "").Replace("nd", "").Replace("rd", "").Replace("th", "").Trim() + " " + DateTime.Now.Year;
                

                CultureInfo provider = CultureInfo.InvariantCulture;
                flyer.EventDates.Add(DateTime.ParseExact(dateText, "d MMMM yyyy", provider));
                if (flyer.EventDates.First() < eventDateStart || flyer.EventDates.First() > eventDateEnd)
                    return null;

                flyer.SourceDetailPage = new Uri(entry.FindElement(By.CssSelector(".gig.name a")).GetAttribute("href"));

                return flyer;


            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                return null;
            }

        }
    }
}
