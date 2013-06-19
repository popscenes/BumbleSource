using System.Collections.Generic;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Query
{
    public class FindFlyersByLatestQueryHandler : QueryHandlerInterface<FindFlyersByLatestQuery, List<FlyerSummaryModel>>
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly GenericQueryServiceInterface _queryService;

        public FindFlyersByLatestQueryHandler(QueryChannelInterface queryChannel, GenericQueryServiceInterface queryService)
        {
            _queryChannel = queryChannel;
            _queryService = queryService;
        }

        public List<FlyerSummaryModel> Query(FindFlyersByLatestQuery argument)
        {
            var ids = _queryChannel.Query(argument, new List<string>());
            var fliers = _queryService.FindByIds<Flier>(ids);
            return _queryChannel.ToViewModel<FlyerSummaryModel, Flier>(fliers);
        }
    }
}