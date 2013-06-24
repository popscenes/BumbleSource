using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Models.Board;
using Website.Common.Controller;
using Website.Common.Extension;
using Website.Infrastructure.Command;

namespace PostaFlya.Controllers
{
    public class BoardFlierController : WebApiControllerBase
    {
        private readonly CommandBusInterface _commandBus;

        public BoardFlierController(CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public HttpResponseMessage Post(string browserId, AddBoardFlierModel addBoardFlierModel)
        {
            return Post(browserId, new List<AddBoardFlierModel>() {addBoardFlierModel});
        }

        public HttpResponseMessage Post(string browserId, List<AddBoardFlierModel> addFlierModel)
        {

            return null;
        }
    }
}