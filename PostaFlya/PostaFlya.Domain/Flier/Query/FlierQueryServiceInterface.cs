using System.Collections.Generic;
using WebSite.Infrastructure.Query;
using Website.Domain.Browser.Query;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.Domain.Flier.Query
{
    public interface FlierQueryServiceInterface : QueryServiceWithBrowserInterface
    {
        IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0);                                                                
    }
}
