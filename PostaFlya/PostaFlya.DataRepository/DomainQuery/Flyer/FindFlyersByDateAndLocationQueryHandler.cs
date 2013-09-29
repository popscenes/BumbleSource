using System.Collections.Generic;
using System.Linq;
using PostaFlya.DataRepository.Indexes;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util;

namespace PostaFlya.DataRepository.DomainQuery.Flyer
{
    public class FindFlyersByDateAndLocationQueryHandler : QueryHandlerInterface<FindFlyersByDateAndLocationQuery, List<Flier>>
    {

       // private readonly FlierSearchServiceInterface _searchService;
        private readonly QueryChannelInterface _queryChannel;
        private readonly TableIndexServiceInterface _indexService;


        public FindFlyersByDateAndLocationQueryHandler(//FlierSearchServiceInterface searchService
            QueryChannelInterface queryChannel
            , TableIndexServiceInterface indexService)
        {
            //_searchService = searchService;
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

            if (string.IsNullOrWhiteSpace(argument.Location.Id)) return null;

            var low = argument.Location.Id.ToStorageKeySection() + startDate.GetTimestampAscending().ToStorageKeySection();
            var high = argument.Location.Id.ToStorageKeySection() + endDate.GetTimestampAscending().ToStorageKeySection();
            var flyers = _indexService.FindEntitiesByIndexRange<FlierInterface, JsonTableEntry>(
                new FlyerSuburbIndex()
                , low
                , high
                , encodeValue: false);

            var list = (from g in flyers
                        let gc = g.GetEntity<GeoPoints>()
                        let dist = gc.Distance
                        let metres = argument.Distance * 1000
                        orderby dist
                        where dist <= metres
                        select g
           ).ToList();

            var queries = list.Select(f => f.RowKey.ExtractEntityIdFromRowKey())
                              .Distinct().Select(id => new FindByIdQuery<Flier>(){Id = id});

            return queries.Select(q => _queryChannel.Query(q, (Flier) null, o => o.CacheFor(1000.Minutes())))
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