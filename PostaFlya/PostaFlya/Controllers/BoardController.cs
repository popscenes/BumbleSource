using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Domain.Boards;
using PostaFlya.Models.Board;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class BoardController : Controller
    {
        private readonly GenericQueryServiceInterface _queryService;

        private readonly QueryChannelInterface _queryChannel;

        public BoardController(GenericQueryServiceInterface queryService, QueryChannelInterface queryChannel)
        {
            _queryService = queryService;
            _queryChannel = queryChannel;
        }

        public ActionResult Get(string boardId)
        {
            var board = _queryService.FindByFriendlyId<Board>(boardId);
            if (board == null)
                return HttpNotFound();

            var ret = _queryChannel.ToViewModel<BoardViewModel>(board);
            return View(ret);
        }

    }
}
