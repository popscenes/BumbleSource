using System.Collections.Generic;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public interface FlierQueryServiceInterface : QueryServiceWithBrowserInterface
    {
        IList<string> FindFliersByLocationTagsAndDistance(Location.Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0);                                                                
    }
}
