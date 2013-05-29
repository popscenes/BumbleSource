using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Domain.Boards;
using PostaFlya.Models.Board;
using Website.Common.Model.Query;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class BoardController : Controller
    {
        private readonly GenericQueryServiceInterface _queryService;

        private readonly QueryChannelInterface _queryChannel;
        private readonly ConfigurationServiceInterface _configurationService;

        public BoardController(GenericQueryServiceInterface queryService, QueryChannelInterface queryChannel, ConfigurationServiceInterface configurationService)
        {
            _queryService = queryService;
            _queryChannel = queryChannel;
            _configurationService = configurationService;
        }

        public ActionResult Get(string id)
        {
            var board = _queryService.FindByFriendlyId<Board>(id);
            if (board == null)
                return HttpNotFound();

            var ret = _queryChannel.ToViewModel<BoardPageViewModel>(board);
            return View(ret);
        }

        public ActionResult Widget(string id)
        {
            Response.ContentType = "text/javascript";
            return View("Widget/Widget", new BoardWidgetViewModel()
                {
                    BoardFriendlyId = id,
                    SiteBase = _configurationService.GetSetting("SiteUrl")
                });
        }

    }
}
