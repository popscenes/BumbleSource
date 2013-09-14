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
        private readonly GenericQueryServiceInterface _queryService;
        private readonly TableIndexServiceInterface _indexService;

        public FindFlyersByBoardQueryHandler(//FlierSearchServiceInterface searchService, 
            GenericQueryServiceInterface queryService
            , TableIndexServiceInterface indexService)
        {
            //_searchService = searchService;
            _queryService = queryService;
            _indexService = indexService;
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

            return _queryService
                .FindByIds<Flier>(flyers
                    .Select(f => f.RowKey.ExtractEntityIdFromRowKey()).Distinct())
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