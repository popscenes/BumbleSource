using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Query
{
    public class FindFlyersByLocationAndDistanceQueryHandler : QueryHandlerInterface<FindFlyersByLocationAndDistanceQuery, List<FlyerSummaryModel>>
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly GenericQueryServiceInterface _queryService;

        public FindFlyersByLocationAndDistanceQueryHandler(QueryChannelInterface queryChannel, GenericQueryServiceInterface queryService)
        {
            _queryChannel = queryChannel;
            _queryService = queryService;
        }

        public List<FlyerSummaryModel> Query(FindFlyersByLocationAndDistanceQuery argument)
        {
            var ids = _queryChannel.Query(argument, new List<string>());
            var fliers = _queryService.FindByIds<Flier>(ids);
            return _queryChannel.ToViewModel<FlyerSummaryModel, Flier>(fliers);
        }
    }
}