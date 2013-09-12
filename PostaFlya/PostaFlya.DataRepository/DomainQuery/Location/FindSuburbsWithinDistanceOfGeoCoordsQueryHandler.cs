using System.Collections.Generic;
using System.Linq;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Indexes;
using PostaFlya.DataRepository.Search.SearchRecord;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Location
{
    public class FindSuburbsWithinDistanceOfGeoCoordsQueryHandler
        : QueryHandlerInterface<FindSuburbsWithinDistanceOfGeoCoordsQuery, List<Suburb>>
    {
        private readonly TableIndexServiceInterface _indexService;
        private readonly QueryChannelInterface _queryChannel;

        public FindSuburbsWithinDistanceOfGeoCoordsQueryHandler(TableIndexServiceInterface indexService, QueryChannelInterface queryChannel)
        {
            _indexService = indexService;
            _queryChannel = queryChannel;
        }

        public List<Suburb> Query(FindSuburbsWithinDistanceOfGeoCoordsQuery argument)
        {
            var point = argument.Geo.ToGeography();
            var box = point.BoundingBoxFromBuffer(argument.Kilometers);
            var low = box.Min;
            var high = box.Max;


            var geos = _indexService.FindEntitiesByIndexRange<SuburbEntityInterface, JsonTableEntry>(
                new SuburbGeoSearchIndex()
                , SuburbGeoSearchIndex.ToGeoLongSearchKey(low.Longitude)
                , SuburbGeoSearchIndex.ToGeoLongSearchKey(high.Longitude)
                , SuburbGeoSearchIndex.ToGeoLatSearchKey(low.Latitude)
                , SuburbGeoSearchIndex.ToGeoLatSearchKey(high.Latitude), encodeValue: false);

            var list = (from g in geos
                        let gc = g.GetEntity<GeoCoords>().ToGeography()
                        let dist = gc.STDistance(point).Value
                        let metres = argument.Kilometers * 1000
                        where dist <= metres                        
                        select g
                       ).ToList();

            var ret =  _queryChannel.Query(new FindByIdsQuery<Suburb>() { Ids = list.Select(r => r.RowKey.ExtractEntityIdFromRowKey()) },
                                           default(List<Suburb>));
            return ret;
        }
    }
}