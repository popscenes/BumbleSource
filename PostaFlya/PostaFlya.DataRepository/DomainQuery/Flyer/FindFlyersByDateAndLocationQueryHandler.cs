using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Indexes;
using PostaFlya.DataRepository.Search.SearchRecord;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Flyer
{
    public class FindFlyersByDateAndLocationQueryHandler : QueryHandlerInterface<FindFlyersByDateAndLocationQuery, List<Flier>>
    {

       // private readonly FlierSearchServiceInterface _searchService;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly QueryChannelInterface _queryChannel;
        private readonly TableIndexServiceInterface _indexService;


        public FindFlyersByDateAndLocationQueryHandler(//FlierSearchServiceInterface searchService
            GenericQueryServiceInterface queryService
            , QueryChannelInterface queryChannel
            , TableIndexServiceInterface indexService)
        {
            //_searchService = searchService;
            _queryService = queryService;
            _queryChannel = queryChannel;
            _indexService = indexService;
        }


        public List<Flier> Query(FindFlyersByDateAndLocationQuery argument)
        {
            return FliersFromIndex(argument);
            //return  FliersFromSearchService(argument);
        }

        private List<Flier> FliersFromIndex(FindFlyersByDateAndLocationQuery argument)
        {
            var startDate = argument.Start.DateTime.Date;
            var endDate = argument.End.AddDays(1).DateTime.Date;

            if (string.IsNullOrWhiteSpace(argument.Location.Id))
            {
                if(!argument.Location.IsValid())
                    return null;

                argument.Location =
                    _queryChannel.Query(
                        new FindNearestSuburbByGeoCoordsQuery() {Geo = argument.Location.AsGeoCoords()},
                        argument.Location);
            }

            if (string.IsNullOrWhiteSpace(argument.Location.Id)) return null;

            if(!argument.Location.IsValid())
                argument.Location =
                    _queryChannel.Query(
                        new FindByIdQuery<Suburb>() { Id = argument.Location.Id},
                        argument.Location);

            if (!argument.Location.IsValid()) return null;


            var low = argument.Location.Id.ToStorageKeySection() + startDate.GetTimestampAscending().ToStorageKeySection();
            var high = argument.Location.Id.ToStorageKeySection() + endDate.GetTimestampAscending().ToStorageKeySection();
            var flyers = _indexService.FindEntitiesByIndexRange<FlierInterface, JsonTableEntry>(
                new FlyerSuburbIndex()
                , low
                , high
                , encodeValue: false);

            var point = argument.Location.ToGeography();
            var list = (from g in flyers
                        let gc = g.GetEntity<GeoCoords>().ToGeography()
                        let dist = gc.STDistance(point).Value
                        let metres = argument.Distance * 1000
                        //orderby dist
                        where dist <= metres
                        select g
           ).ToList();

            return _queryService
                .FindByIds<Flier>(list
                    .Select(f => f.RowKey.ExtractEntityIdFromRowKey()).Distinct())
                .ToList();
        }

//        private List<Flier> FliersFromSearchService(FindFlyersByDateAndLocationQuery argument)
//        {
//            if (!argument.Location.IsValid() && !string.IsNullOrWhiteSpace(argument.Location.Id))
//                argument.Location = _queryChannel.Query(new FindByIdQuery<Suburb>() { Id = argument.Location.Id }, argument.Location);
//
//            if (!argument.Location.IsValid())
//                return default(List<Flier>);
//
//            var startDate = argument.Start.DateTime.Date;
//            var endDate = argument.End.DateTime.Date;
//            if (argument.Start.Offset == TimeSpan.Zero)
//            {
//                startDate = argument.Start.UtcDateTime.Date;
//                endDate = argument.End.UtcDateTime.Date;
//            }
//
//            var loc = new Website.Domain.Location.Location();
//            loc.CopyFieldsFrom(argument.Location);
//            var ids = _searchService.FindFliersByLocationAndDate(loc, argument.Distance, startDate,
//                                                                 endDate);
//
//            return _queryService.FindByIds<Flier>(ids).ToList();
//        }
    }
}