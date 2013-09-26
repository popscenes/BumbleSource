using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Indexes;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Azure.Common.TableStorage;
using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Flyer
{
    public class FindFlyersByBoardQueryHandler : QueryHandlerInterface<FindFlyersByBoardQuery, List<Flier>>
    {

        //private readonly FlierSearchServiceInterface _searchService;
        private readonly TableIndexServiceInterface _indexService;
        private readonly QueryChannelInterface _queryChannel;


        public FindFlyersByBoardQueryHandler(//FlierSearchServiceInterface searchService, 
            TableIndexServiceInterface indexService, QueryChannelInterface queryChannel)
        {
            //_searchService = searchService;
            _indexService = indexService;
            _queryChannel = queryChannel;
        }


        private List<Flier> FindFromIndex(FindFlyersByBoardQuery argument)
        {
            var startDate = argument.Start.DateTime.Date;
            var endDate = argument.End.AddDays(1).DateTime.Date;

            var low = argument.BoardId.ToStorageKeySection() + startDate.GetTimestampAscending().ToStorageKeySection();
            var high = argument.BoardId.ToStorageKeySection() + endDate.GetTimestampAscending().ToStorageKeySection();
            var flyers = _indexService.FindEntitiesByIndexRange<FlierInterface, JsonTableEntry>(
                new FlyerBoardsIndex()
                , low
                , high
                , encodeValue: false);


            var queries = flyers.Select(f => f.RowKey.ExtractEntityIdFromRowKey())
                              .Distinct().Select(id => new FindByIdQuery<Flier>() { Id = id });

            return queries.Select(q => _queryChannel.Query(q, (Flier)null, o => o.CacheFor(1000.Minutes())))
                          .ToList();
        }

//        private List<Flier> FindFromSearchService(FindFlyersByBoardQuery argument)
//        {
//            var startDate = argument.Start.DateTime.Date;
//            var endDate = argument.End.DateTime.Date;
//            if (argument.Start.Offset == TimeSpan.Zero)
//            {
//                startDate = argument.Start.UtcDateTime.Date;
//                endDate = argument.End.UtcDateTime.Date;
//            }
//            var ids = _searchService.FindFlyersByBoard(argument.BoardId, startDate,
//                                                       endDate);
//
//            return _queryService.FindByIds<Flier>(ids).ToList();
//        }
            
        public List<Flier> Query(FindFlyersByBoardQuery argument)
        {
            //return FindFromSearchService(argument);
            return FindFromIndex(argument);
        }
    }
}