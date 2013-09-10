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
                    DomainIndexSelectors.GeoSearchIndex
                    , low.Longitude.ToGeoLongSearchKey()
                    , high.Longitude.ToGeoLongSearchKey()
                    , low.Latitude.ToGeoLatSearchKey()
                    , high.Latitude.ToGeoLatSearchKey(), encodeValue: false);

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
                    DomainIndexSelectors.GeoSearchIndex
                    , low.Longitude.ToGeoLongSearchKey()
                    , high.Longitude.ToGeoLongSearchKey()
                    , low.Latitude.ToGeoLatSearchKey()
                    , high.Latitude.ToGeoLatSearchKey(), encodeValue: false);


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
