using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Flyer
{
    public class FindFlyersByFeaturedQueryHandler : QueryHandlerInterface<FindFlyersByFeaturedQuery, List<string>>
    {
        private readonly FlierSearchServiceInterface _searchService;
        private readonly GenericQueryServiceInterface _queryService;

        public FindFlyersByFeaturedQueryHandler(FlierSearchServiceInterface searchService, GenericQueryServiceInterface queryService)
        {
            _searchService = searchService;
            _queryService = queryService;
        }

        public List<string> Query(FindFlyersByFeaturedQuery argument)
        {
            var ret =
                _searchService.FindFliersByLocationAndDistance(null, 0, argument.Take
                , !string.IsNullOrWhiteSpace(argument.Skip) ? _queryService.FindById<Flier>(argument.Skip) : null);

            return ret.ToList();
        }
    }
}