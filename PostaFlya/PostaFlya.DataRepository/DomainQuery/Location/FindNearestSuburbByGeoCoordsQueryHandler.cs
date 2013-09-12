using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Indexes;
using PostaFlya.DataRepository.Search.SearchRecord;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Location
{
    public class FindNearestSuburbByGeoCoordsQueryHandler : QueryHandlerInterface<FindNearestSuburbByGeoCoordsQuery, Suburb>
    {
        private readonly TableIndexServiceInterface _indexService;
        private readonly QueryChannelInterface _queryChannel;

        public FindNearestSuburbByGeoCoordsQueryHandler(TableIndexServiceInterface indexService, QueryChannelInterface queryChannel)
        {
            _indexService = indexService;
            _queryChannel = queryChannel;
        }

        public Suburb Query(FindNearestSuburbByGeoCoordsQuery argument)
        {
            Suburb ret = null;
            var point = argument.Geo.ToGeography();
            var start = 2;
            do
            {
                ret = FindSuburbWithin(point, start);
                start *= 2;
            } while (ret == null && start < 64);

            return ret;
        }

        private Suburb FindSuburbWithin(SqlGeography target, int kilometers)
        {

            var box = target.BoundingBoxFromBuffer(kilometers);
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
                        let dist = gc.STDistance(target)
                        orderby dist
                        select new { g, dist }).ToList();

            var ret = list.FirstOrDefault();
            return ret == null
                       ? null
                       : _queryChannel.Query(new FindByIdQuery<Suburb>() { Id = ret.g.RowKey.ExtractEntityIdFromRowKey() },
                                             default(Suburb));
        }
    }
}
