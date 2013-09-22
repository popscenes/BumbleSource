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
        private readonly QueryChannelInterface _queryChannel;
        private readonly ConfigurationServiceInterface _configurationService;

        public BoardController(QueryChannelInterface queryChannel, ConfigurationServiceInterface configurationService)
        {
            _queryChannel = queryChannel;
            _configurationService = configurationService;
        }

        public ActionResult Get(string id)
        {
            var board = _queryChannel.Query(new FindByFriendlyIdQuery<Board>() { FriendlyId = id }, (BoardPageViewModel)null);
            if (board == null)
                return HttpNotFound();

            return View(board);
        }

        public ActionResult Widget(string id)
        {
            Response.ContentType = "text/javascript";

            var board = _queryChannel.Query(new FindByFriendlyIdQuery<Board>() { FriendlyId = id }, (BoardPageViewModel)null);
            if (board == null)
                return HttpNotFound();

            return View("Widget/Widget", new BoardWidgetViewModel()
                {
                    BoardFriendlyId = board.FriendlyId,
                    BoardId = board.Id,
                    SiteBase = _configurationService.GetSetting("SiteUrl")
                });
        }

    }
}
