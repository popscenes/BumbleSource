using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using Website.Application.Binding;
using Website.Application.Domain.Browser;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Domain;

namespace PostaFlya.Controllers
{
    public class TinyUrlController : Controller
    {
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly CommandBusInterface _commandBus;
        private readonly BrowserInformationInterface _browserInformation;

        public TinyUrlController(ConfigurationServiceInterface configurationService, [WorkerCommandBus]CommandBusInterface commandBus
            , BrowserInformationInterface browserInformation)
        {
            _configurationService = configurationService;
            _commandBus = commandBus;
            _browserInformation = browserInformation;
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


            var s = QrSource.GetDescFromParam(Request.Params["q"]) ?? "tinyurl";
            var routeVals = new {id = info.FriendlyId, t = Guid.NewGuid().ToString()};

            _commandBus.Send(new FlierAnalyticCommand()
                {
                    FlierId = info.Id,
                    TrackingId = routeVals.t,
                    IpAddress = _browserInformation.IpAddress,
                    UserAgent = _browserInformation.UserAgent,
                    Source = s
                });

            return Redirect(siteUrl + Url.Action("Index", "TrackView", routeVals));
        }

    }
}
