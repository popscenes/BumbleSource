using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Models.Board;
using Website.Common.Controller;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class BoardSearchApiController : WebApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;

        public BoardSearchApiController(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public List<BoardModel> Get([FromUri]BoardSearchModel req)
        {
            var list =
                _queryChannel.Query(new FindBoardsNearQuery()
                    {
                        Location = req.Loc.ToDomainModel(),
                        Take = req.Count,
                        Skip = req.Skip,
                        WithinMetres = req.Distance
                    }, new List<Board>());

            return _queryChannel.ToViewModel<BoardModel, Board>(list);
        } 
    }
}
