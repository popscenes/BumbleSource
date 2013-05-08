using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScraper.Library.Model;

namespace WebScraper.Library.Infrastructure
{
    public interface SiteScraperInterface
    {
        string SiteName { get; }
        List<ImportedFlyerScraperModel> GetFlyersFrom(DateTime eventDateStart, DateTime eventDateEnd);
    }
}
