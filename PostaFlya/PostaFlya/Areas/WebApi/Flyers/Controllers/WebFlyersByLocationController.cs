using System;
using System.Web.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Areas.WebApi.Flyers.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Domain.Location;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.WebApi.Flyers.Controllers
{
    public class WebFlyersByLocationController : WebApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly ConfigurationServiceInterface _config;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;

        public WebFlyersByLocationController(QueryChannelInterface queryChannel, ConfigurationServiceInterface config, PostaFlyaBrowserInformationInterface browserInformation)
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
