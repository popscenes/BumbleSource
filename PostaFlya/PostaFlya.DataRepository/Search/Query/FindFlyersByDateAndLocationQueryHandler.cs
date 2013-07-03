using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Search.Query
{
    public class FindFlyersByDateAndLocationQueryHandler : QueryHandlerInterface<FindFlyersByDateAndLocationQuery, List<Flier>>
    {

        private readonly FlierSearchServiceInterface _searchService;
        private readonly GenericQueryServiceInterface _queryService;

        public FindFlyersByDateAndLocationQueryHandler(FlierSearchServiceInterface searchService, GenericQueryServiceInterface queryService)
        {
            _searchService = searchService;
            _queryService = queryService;
        }


        public List<Flier> Query(FindFlyersByDateAndLocationQuery argument)
        {
            var startDate = argument.Start.DateTime;
            var endDate = argument.End.DateTime;
            if (argument.Start.Offset == TimeSpan.Zero)
            {
                startDate = argument.Start.UtcDateTime;
                endDate = argument.End.UtcDateTime;
            }
            var ids = _searchService.FindFliersByLocationAndDate(argument.Location, argument.Distance, startDate,
                                                                 endDate);

            return _queryService.FindByIds<Flier>(ids).ToList();
        }
    }
}