using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Areas.MobileApi.Infrastructure.Controller;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Domain.Location;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.MobileApi.Flyers.Controllers
{

    public class FlyersByLocationController : MobileApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly ConfigurationServiceInterface _config;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;

        public FlyersByLocationController(QueryChannelInterface queryChannel, ConfigurationServiceInterface config, PostaFlyaBrowserInformationInterface browserInformation)
        {
            _queryChannel = queryChannel;
            _config = config;
            _browserInformation = browserInformation;
        }

        public ResponseContent<FlyersByDateContent> Get([FromUri]FlyersByLocationRequest req)
        {
            var start = req.Start != default(DateTimeOffset) ? req.Start : DateTimeOffset.UtcNow;
            var query = new FindFlyersByDateAndLocationQuery()
                {
                    Location = new Location(req.Lng, req.Lat),
                    Distance = req.Distance,
                    Start = start,
                    End = req.End != default(DateTimeOffset) ? req.End : start.AddDays(_config.GetSetting("DaySpan", 7))
                };
            var content = _queryChannel.Query(query, new FlyersByDateContent());

            _browserInformation.LastSearchLocation = query.Location;

            return ResponseContent<FlyersByDateContent>.GetResponse(content);
        }
    }
}
