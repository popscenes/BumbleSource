using System;
using System.Collections.Generic;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier.Query
{

    public interface FlierSearchServiceInterface
    {
        IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, string board = null, int distance = 0, int minTake = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0);
        IList<string> IterateAllIndexedFliers(int minTake, int nextSkip, bool returnFriendlyId = false);
    }
}
