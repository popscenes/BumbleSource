using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Domain.Flier.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class TinyUrlController : Controller
    {
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly QueryChannelInterface _queryChannel;

        public TinyUrlController(ConfigurationServiceInterface configurationService, 
            QueryChannelInterface queryChannel)
        {
            _configurationService = configurationService;
            _queryChannel = queryChannel;
        }

        public ActionResult Get(string path)
        { 
            object fromRoute = null;
            if (!this.ControllerContext.RouteData.Values.TryGetValue("tinyUrlEntityInfo", out fromRoute))
                fromRoute = this.ControllerContext.HttpContext.Items["tinyUrlEntityInfo"];

            var info = fromRoute as EntityInterface;
            if(info == null || !(info.PrimaryInterface == typeof(FlierInterface)))
                return new HttpNotFoundResult();
            var flier = _queryChannel.Query(new FindByIdQuery<Flier>() {Id = info.Id},
                                            (Flier) null);
            if(flier == null)
                return new HttpNotFoundResult();

            var siteUrl = _configurationService.GetSetting("SiteUrl") ?? "";

            var routeVals = new { id = flier.FriendlyId, src = HttpContext.Request.Url.ToString() };

            return Redirect(siteUrl + Url.Action("Index", "TrackView", routeVals));
        }

    }
}
