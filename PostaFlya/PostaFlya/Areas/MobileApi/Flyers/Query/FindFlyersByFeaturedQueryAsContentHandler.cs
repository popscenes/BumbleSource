using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Query
{
    public class FindFlyersByFeaturedQueryAsContentHandler : QueryHandlerInterface<FindFlyersByFeaturedQuery, FlyersByFeaturedContent>
    {
        private readonly QueryChannelInterface _queryChannel;

        public FindFlyersByFeaturedQueryAsContentHandler(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public FlyersByFeaturedContent Query(FindFlyersByFeaturedQuery argument)
        {
            var flyers = _queryChannel.Query(argument, new List<FlyerSummaryModel>());

            return new FlyersByFeaturedContent()
            {
                FeatureGroups = new List<FlyersByFeaturedContent.FlyersByFeature>()
                    {
                        new FlyersByFeaturedContent.FlyersByFeature()
                            {
                                FeatureGroup = "Latest",
                                FlyerIds = flyers.Select(model => model.Id).ToList()
                            }
                    },
                Flyers = flyers.ToDictionary(model => model.Id)
            };
        }
    }
}