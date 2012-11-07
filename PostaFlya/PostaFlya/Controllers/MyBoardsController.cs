using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Models.Board;
using Website.Common.Extension;
using Website.Infrastructure.Command;

namespace PostaFlya.Controllers
{
    public class MyBoardsController : ApiController
    {
        private readonly CommandBusInterface _commandBus;

        public MyBoardsController(CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public HttpResponseMessage Post(string browserId, BoardCreateEditModel boardCreate)
        {
            var createBoardCommand = new CreateBoardCommand()
                {
                    BrowserId = browserId,
                    BoardName = boardCreate.BoardName,
                    Location = boardCreate.Location != null ? boardCreate.Location.ToDomainModel() : null,
                    AllowOthersToPostFliers = boardCreate.AllowOthersToPostFliers,
                    RequireApprovalOfPostedFliers = boardCreate.RequireApprovalOfPostedFliers
                };

            var res = _commandBus.Send(createBoardCommand);
            return this.GetResponseForRes(res);
        }
    }
}