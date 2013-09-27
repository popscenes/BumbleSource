using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Location
{
    public class PopulateSuburbQueryHandler
        : QueryHandlerInterface<PopulateSuburbQuery, Suburb>
    {
        private readonly QueryChannelInterface _queryChannel;

        public PopulateSuburbQueryHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public Suburb Query(PopulateSuburbQuery argument)
        {
            var ret = argument.Suburb;
            if (string.IsNullOrWhiteSpace(ret.Id))
            {
                if (!ret.IsValid())
                    return null;

                ret = _queryChannel.Query(
                        new FindNearestSuburbByGeoCoordsQuery() { Geo = ret.AsGeoCoords() },
                        ret);
            }

            return string.IsNullOrWhiteSpace(ret.Id) ? null : ret;
        }
    }
}