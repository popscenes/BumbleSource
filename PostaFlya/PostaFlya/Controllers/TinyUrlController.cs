using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Domain.Flier;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Domain;

namespace PostaFlya.Controllers
{
    public class TinyUrlController : Controller
    {
        private readonly ConfigurationServiceInterface _configurationService;

        public TinyUrlController(ConfigurationServiceInterface configurationService)
        {
            _configurationService = configurationService;
        }

        public ActionResult Get(string path)
        { 
            object fromRoute = null;
            if (!this.ControllerContext.RouteData.Values.TryGetValue("tinyUrlEntityInfo", out fromRoute))
                fromRoute = this.ControllerContext.HttpContext.Items["tinyUrlEntityInfo"];

            var info = fromRoute as EntityInterface;
            if(info == null || !(info.PrimaryInterface == typeof(FlierInterface)))
                return new HttpNotFoundResult();

            var siteUrl = _configurationService.GetSetting("SiteUrl") ?? "";

            return RedirectPermanent(siteUrl + "/" + info.FriendlyId);
        }

    }
}
