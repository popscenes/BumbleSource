using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper.Library.Infrastructure;
using WebScraper.Library.Model;

namespace WebScraper.Library.Sites
{
    class GraceDarlingScraper : SiteScraperInterface
    {
        public string SiteName { get { return RegisterSites.GraceDarling; } }

        public const string BaseUrl = "http://thedrunkenpoet.com.au";
        private const string Url = BaseUrl + "/whats_on";
        private const string Tags = "music";

        public List<ImportedFlyerScraperModel> GetFlyersFrom(DateTime eventDateStart, DateTime eventDateEnd)
        {
            throw new NotImplementedException();
        }
    }
}
