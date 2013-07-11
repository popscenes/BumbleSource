using System.Collections.Generic;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public class FindFlyersByLocationAndDistanceQuery : QueryInterface<Flier>
    {
        public Location Location { get; set; }

        public int Distance { get; set; }

        public int Take { get; set; }

        public string Skip { get; set; }
    }

    public class FindFlyersByLocationAndDistanceQueryHandler : QueryHandlerInterface<FindFlyersByLocationAndDistanceQuery, List<Flier>>
    {
        private readonly QueryChannelInterface _queryChannel;

        public FindFlyersByLocationAndDistanceQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public List<Flier> Query(FindFlyersByLocationAndDistanceQuery argument)
        {
            var ret = _queryChannel.Query(argument, new List<string>());
            return _queryChannel.Query(new FindByIdsQuery<Flier> {Ids = ret}, (List<Flier>)null);

        }
    }
}