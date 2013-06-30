using System.Collections.Generic;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public class FindFlyersByLatestQuery : QueryInterface<Flier>
    {
        public int Take { get; set; }

        public string Skip { get; set; }
    }

    public class FindFlyersByFeaturedQueryHandler : QueryHandlerInterface<FindFlyersByLatestQuery, List<Flier>>
    {
        private readonly QueryChannelInterface _queryChannel;

        public FindFlyersByFeaturedQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public List<Flier> Query(FindFlyersByLatestQuery argument)
        {
            var ret = _queryChannel.Query(argument, new List<string>());
            return _queryChannel.Query(new FindByIdsQuery<Flier> { Ids = ret }, (List<Flier>)null);

        }
    }
}