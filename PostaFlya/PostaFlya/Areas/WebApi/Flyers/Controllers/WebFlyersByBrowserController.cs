using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using PostaFlya.Areas.WebApi.Flyers.Model;
using PostaFlya.Areas.WebApi.Flyers.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Domain.Browser.Query;
using Website.Domain.Claims;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.WebApi.Flyers.Controllers
{
    public class WebFlyersByBrowserController : WebApiControllerBase
    {
        //
        // GET: /WebApi/WebFlyersByBrowser/

        private readonly QueryChannelInterface _queryChannel;

        public WebFlyersByBrowserController(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public ResponseContent<FlyersByDateContent> Get([FromUri]FlyersByBrowserRequest req)
        {
            var query = new GetByBrowserIdQuery<Claim>() {BrowserId = req.BrowserId};

            var content = _queryChannel.Query(query, new FlyersByDateContent());
            return ResponseContent<FlyersByDateContent>.GetResponse(content);
        }

    }
}
