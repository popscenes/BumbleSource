using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Search.Query
{
    public class FindFlyersByLocationAndDistanceQueryHandler : QueryHandlerInterface<FindFlyersByLocationAndDistanceQuery, List<string>>
    {
        private readonly FlierSearchServiceInterface _searchService;

        public FindFlyersByLocationAndDistanceQueryHandler(FlierSearchServiceInterface searchService)
        {
            _searchService = searchService;
        }

        public List<string> Query(FindFlyersByLocationAndDistanceQuery argument)
        {
            var ret = 
                _searchService.FindFliersByLocationAndDistance(argument.Location, argument.Distance, argument.Take);
            
            return ret.ToList();
        }
    }
}