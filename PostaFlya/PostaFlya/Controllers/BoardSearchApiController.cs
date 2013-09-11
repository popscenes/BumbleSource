using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Areas.WebApi.Location.Model;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Models.Board;
using Website.Common.Model.Query;
using Website.Common.Obsolete;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class BoardSearchApiController : OldWebApiControllerBase
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
                        Location = _queryChannel.ToViewModel<Suburb, SuburbModel>(req.Loc),
                        Take = req.Count,
                        Skip = req.Skip,
                        WithinMetres = req.Distance
                    }, new List<Board>());

            return _queryChannel.ToViewModel<BoardModel, Board>(list);
        } 
    }
}
