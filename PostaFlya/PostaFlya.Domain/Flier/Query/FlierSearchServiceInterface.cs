using System;
using System.Collections.Generic;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.Flier.Query
{

    public interface FlierSearchServiceInterface
    {
        IList<string> FindFliersByBoard(string board, int take, FlierInterface skipPast = null, Tags tags = null, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, Location location = null, int distance = 5); 
        IList<string> FindFliersByLocationAndDistance(Location location, int distance, int take, FlierInterface skipPast = null, Tags tags = null, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate);
        IList<string> IterateAllIndexedFliers(int take, FlierInterface skipPast, bool returnFriendlyId = false, Tags tags = null);
    }
}
