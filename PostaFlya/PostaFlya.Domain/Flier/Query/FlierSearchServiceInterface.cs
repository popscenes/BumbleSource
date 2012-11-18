using System.Collections.Generic;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.Domain.Flier.Query
{
    public interface FlierSearchServiceInterface
    {
        IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, string board = null, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0);  
    }
}
