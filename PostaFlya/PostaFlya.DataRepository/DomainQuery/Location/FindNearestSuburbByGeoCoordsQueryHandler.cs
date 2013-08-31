using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;
using PostaFlya.DataRepository.Binding;
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
            var start = 10;
            do
            {
                ret = FindSuburbWithin(point, start);
                start += 10;
            } while (ret == null);

            return ret;
        }

        private Suburb FindSuburbWithin(SqlGeography target, int kilometers)
        {

            var box = target.BoundingBoxFromBuffer(kilometers);
            var low = box.Min;
            var high = box.Max;

            var geos = _indexService.FindEntitiesByIndexRange<SuburbEntityInterface, JsonTableEntry>(
                    DomainIndexSelectors.GeoSearchIndex
                    , low.Longitude.ToGeoLongSearchKey()
                    , high.Longitude.ToGeoLongSearchKey()
                    , low.Latitude.ToGeoLatSearchKey()
                    , high.Latitude.ToGeoLatSearchKey(), encodeValue: false);


            var ret = (from g in geos
                       let gc = g.GetEntity<GeoCoords>().ToGeography()
                       orderby gc.STDistance(target)
                       select g).FirstOrDefault();


            return ret == null
                       ? null
                       : _queryChannel.Query(new FindByIdQuery<Suburb>() { Id = ret.RowKey.ExtractEntityIdFromRowKey() },
                                             default(Suburb));
        }
    }
}
