using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Flier;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.DataRepository.Search.Services
{
    interface FlierSearchServiceInterface
    {
        IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, string board = null, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0);  
    }
}
