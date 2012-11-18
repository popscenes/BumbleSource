using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Attributes;
using Website.Common.Extension;
using Website.Infrastructure.Command;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Tag;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    [BrowserAuthorize(Roles = "Participant")]
    public class SavedTagsController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _queryService;

        public SavedTagsController(CommandBusInterface commandBus
            , GenericQueryServiceInterface queryService)
        {
            _commandBus = commandBus;
            _queryService = queryService;
        }

        // GET /api/savedtagsapi
        public IQueryable<Tags> Get(string browserId)
        {
            var browser = _queryService.FindById<Browser>(browserId);
           return browser.SavedTags.AsQueryable();
        }

        // POST /api/savedtagsapi
        public HttpResponseMessage Post(string browserId, Tags tags)
        {
            var browser = _queryService.FindById<Browser>(browserId);
            var command = new SavedTagsSaveCommand()
            {
                BrowserId = browser.Id,
                Tags = tags
            };

            var res = _commandBus.Send(command);
            return this.GetResponseForRes(res);
        }

        //sets the active saved tags
        public HttpResponseMessage Put(string browserId, Tags tags)
        {
            var browser = _queryService.FindById<Browser>(browserId);
            var command = new SavedTagsSelectCommand()
            {
                BrowserId = browser.Id,
                Tags = new Tags(tags)
            };

            var res = _commandBus.Send(command);
            return this.GetResponseForRes(res);
        }

        // DELETE /api/savedtagsapi/5
        public HttpResponseMessage Delete(string browserId, Tags tags)
        {
            var browser = _queryService.FindById<Browser>(browserId);
            var command = new SavedTagsDeleteCommand()
            {
                BrowserId = browser.Id,
                Tags = new Tags(tags)
            };

            var res = _commandBus.Send(command);
            return this.GetResponseForRes(res);
        }
    }
}
