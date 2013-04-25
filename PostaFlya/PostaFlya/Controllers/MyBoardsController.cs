using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Models.Board;
using Website.Application.Domain.Browser.Web;
using Website.Common.Extension;
using Website.Infrastructure.Command;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeHttp(Roles = "Participant")]
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
                    AllowOthersToPostFliers = boardCreate.AllowOthersToPostFliers,
                    RequireApprovalOfPostedFliers = boardCreate.RequireApprovalOfPostedFliers,
                    BoardTypeEnum = boardCreate.TypeOfBoard != BoardTypeEnum.VenueBoard ? boardCreate.TypeOfBoard : BoardTypeEnum.InterestBoard 
                    
                };

            var res = _commandBus.Send(createBoardCommand);
            return this.GetResponseForRes(res);
        }

        public HttpResponseMessage Put(string browserId, BoardCreateEditModel boardEdit)
        {
            var editBoardCommand = new EditBoardCommand()
            {
                Id = boardEdit.Id,
                BrowserId = browserId,
                BoardName = boardEdit.BoardName,
                AllowOthersToPostFliers = boardEdit.AllowOthersToPostFliers,
                RequireApprovalOfPostedFliers = boardEdit.RequireApprovalOfPostedFliers,
                Status = BoardStatus.Approved
            };

            var res = _commandBus.Send(editBoardCommand);
            return this.GetResponseForRes(res);
        }
    }
}