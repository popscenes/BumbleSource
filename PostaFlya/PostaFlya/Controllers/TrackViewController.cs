using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Models.Location;
using Website.Application.Binding;
using Website.Domain.Location;
using Website.Infrastructure.Command;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class TrackViewController : Controller
    {
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _queryService;


        public TrackViewController(PostaFlyaBrowserInformationInterface browserInformation, ConfigurationServiceInterface configurationService
            , [WorkerCommandBus]CommandBusInterface commandBus, GenericQueryServiceInterface queryService)
        {
            _browserInformation = browserInformation;
            _configurationService = configurationService;
            _commandBus = commandBus;
            _queryService = queryService;
        }

        public ActionResult Index(string t, string id)
        {
            _browserInformation.TrackingId = t;

            var siteUrl = _configurationService.GetSetting("SiteUrl") ?? "";
            return RedirectPermanent(siteUrl + "/" + id);
        }

        public ActionResult Loc(LocationModel loc, string id)
        {
            var flier = _queryService.FindByFriendlyId<Flier>(id.Trim('/'));
            if(flier == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            if (loc != null && loc.IsValid())
            {
                _browserInformation.LastLocation = loc.ToDomainModel();
                _browserInformation.LocationFromDevice = true;
            }

            _commandBus.Send(new FlierAnalyticCommand()
            {
                FlierId = flier.Id,
                TrackingId = _browserInformation.TrackingId,
                IpAddress = _browserInformation.IpAddress,
                UserAgent = _browserInformation.UserAgent,
                Source = "LocationTrack"
            });
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

    }
}
