using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Models.Board;
using Website.Application.Domain.Browser.Web;
using Website.Common.Extension;
using Website.Common.Obsolete;
using Website.Infrastructure.Command;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeHttp(Roles = "Participant")]
    public class MyBoardsController : OldWebApiControllerBase
    {
        private readonly CommandBusInterface _commandBus;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;

        public MyBoardsController(CommandBusInterface commandBus, PostaFlyaBrowserInformationInterface browserInformation)
        {
            _commandBus = commandBus;
            _browserInformation = browserInformation;
        }

        public HttpResponseMessage Post(string browserId, BoardCreateEditModel boardCreate)
        {
            var createBoardCommand = new CreateBoardCommand()
                {

                    BrowserId = browserId,
                    BoardName = boardCreate.BoardName,
                    AllowOthersToPostFliers = boardCreate.AllowOthersToPostFliers,
                    RequireApprovalOfPostedFliers = boardCreate.RequireApprovalOfPostedFliers,
                    BoardTypeEnum = boardCreate.TypeOfBoard,
                    SourceInformation = boardCreate.VenueInformation == null ? null :boardCreate.VenueInformation.ToDomainModel(),
                    AdminEmailAddresses = boardCreate.AdminEmailAddresses.Select(s => s.Trim()).ToList(),
                    Description = boardCreate.Description
                    
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