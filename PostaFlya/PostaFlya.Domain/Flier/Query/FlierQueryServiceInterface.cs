using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Comments.Query;
using PostaFlya.Domain.Likes.Query;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public interface FlierQueryServiceInterface : GenericQueryServiceInterface<FlierInterface>
        , QueryByBrowserInterface<FlierInterface>
        , QueryLikesInterface<FlierInterface>
        , QueryCommentsInterface<FlierInterface>
    {
        IList<string> FindFliersByLocationTagsAndDistance(Location.Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0);                                                                
    }
}
