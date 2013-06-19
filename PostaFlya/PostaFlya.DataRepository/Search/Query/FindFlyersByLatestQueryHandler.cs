using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Search.Query
{
    public class FindFlyersByLatestQueryHandler : QueryHandlerInterface<FindFlyersByLatestQuery, List<string>>
    {
        private readonly FlierSearchServiceInterface _searchService;

        public FindFlyersByLatestQueryHandler(FlierSearchServiceInterface searchService)
        {
            _searchService = searchService;
        }

        public List<string> Query(FindFlyersByLatestQuery argument)
        {
            var ret =
                _searchService.FindFliersByLocationAndDistance(null, 0, argument.Take);

            return ret.ToList();
        }
    }
}