using System;
using System.Web.Http;
using System.Web.Http.Description;
using PostaFlya.Areas.WebApi.Flyers.Model;
using PostaFlya.Domain.Flier.Query;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.WebApi.Flyers.Controllers
{
    public class WebFlyersByBoardController : WebApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;
        private readonly ConfigurationServiceInterface _config;


        public WebFlyersByBoardController(QueryChannelInterface queryChannel, ConfigurationServiceInterface config)
        {
            _queryChannel = queryChannel;
            _config = config;
        }

        public ResponseContent<FlyersByDateContent> Get([FromUri]FlyersByBoardRequest req)
        {
            var start = req.Start != default(DateTimeOffset) ? req.Start : DateTimeOffset.UtcNow;
            var content = _queryChannel.Query(new FindFlyersByBoardQuery()
                {
                    BoardId = req.BoardId,
                    Start = start,
                    End = req.End != default(DateTimeOffset) ? req.End : start.AddDays(_config.GetSetting("DaySpan", 7))
                }, new FlyersByDateContent());

            return ResponseContent<FlyersByDateContent>.GetResponse(content);
        }
    }
}
