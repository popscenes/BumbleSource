using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Attributes;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Location;
using WebSite.Common.Extension;
using WebSite.Infrastructure.Command;
using PostaFlya.Models.Location;

namespace PostaFlya.Controllers
{
    [BrowserAuthorize(Roles = "Participant")]
    public class SavedLocationsController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly BrowserQueryServiceInterface _browserQueryService;

        public SavedLocationsController(BrowserQueryServiceInterface browserQueryService, 
            CommandBusInterface commandBus)
        {
            _browserQueryService = browserQueryService;
            _commandBus = commandBus;
        }


        public IQueryable<LocationModel> Get(string browserId)
        {
            var browser = _browserQueryService.FindById<Domain.Browser.Browser>(browserId);
            return browser.SavedLocations.Select(_ => _.ToViewModel()).AsQueryable();
        }


        public HttpResponseMessage Post(string browserId, LocationModel location)
        {
            var browser = _browserQueryService.FindById<Domain.Browser.Browser>(browserId);

            var command = new SavedLocationAddCommand()
            {
                BrowserId = browser.Id,
                Location = location.ToDomainModel()
            };

            var res = _commandBus.Send(command);

            return this.GetResponseForRes(res);
        }

        //sets the active location
        public HttpResponseMessage Put(string browserId, LocationModel location)
        {
            var browser = _browserQueryService.FindById<Domain.Browser.Browser>(browserId);
            var command = new SavedLocationSelectCommand()
            {
                BrowserId = browser.Id,
                Location = location.ToDomainModel()
            };

            var res = _commandBus.Send(command);

            return this.GetResponseForRes(res);
        }

        public void Delete(string browserId, Location location)
        {
            var browser = _browserQueryService.FindById<Browser>(browserId);
            var command = new SavedLocationDeleteCommand()
            {
                BrowserId = browser.Id,
                Location = location
            };

            _commandBus.Send(command);
        }
    }
}
