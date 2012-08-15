﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Attributes;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Tag;
using WebSite.Common.Extension;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Controllers
{
    [BrowserAuthorize(Roles = "Participant")]
    public class SavedTagsController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly BrowserQueryServiceInterface _browserQueryService;

        public SavedTagsController(CommandBusInterface commandBus
            , BrowserQueryServiceInterface browserQueryService)
        {
            _commandBus = commandBus;
            _browserQueryService = browserQueryService;
        }

        // GET /api/savedtagsapi
        public IQueryable<Tags> Get(string browserId)
        {
           var browser =  _browserQueryService.FindById(browserId);
           return browser.SavedTags.AsQueryable();
        }

        // POST /api/savedtagsapi
        public HttpResponseMessage Post(string browserId, Tags tags)
        {
            var browser = _browserQueryService.FindById(browserId);
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
            var browser = _browserQueryService.FindById(browserId);
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
            var browser = _browserQueryService.FindById(browserId);
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
