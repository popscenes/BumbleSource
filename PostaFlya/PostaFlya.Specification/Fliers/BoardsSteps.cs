using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http;
using NUnit.Framework;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Models;
using PostaFlya.Models.Location;
using PostaFlya.Specification.Util;
using TechTalk.SpecFlow;
using Website.Application.Extension.Validation;
using Website.Common.Extension;
using Website.Infrastructure.Command;

namespace PostaFlya.Specification.Fliers
{
    [Binding]
    public class BoardsSteps
    {
        [When(@"I submit the following data for the BOARD:")]
        public void WhenISubmitTheFollowingDataForTheBOARD(Table table)
        {
            var boardCreate = new BoardCreateEditModel()
                {
                    BoardName = table.Rows[0]["BoardName"],
                    AllowOthersToPostFliers = Boolean.Parse(table.Rows[0]["AcceptOthersFliers"]),
                    RequireApprovalOfPostedFliers = Boolean.Parse(table.Rows[0]["RequireApprovalForFliers"]),
                };

            WhenISubmitTheFollowingDataForTheBOARD(boardCreate);
        }

        public void WhenISubmitTheFollowingDataForTheBOARD(BoardCreateEditModel boardCreate)
        {
            var browserId = SpecUtil.GetCurrBrowser().Browser.Id;

            var controller = SpecUtil.GetApiController<MyBoardsController>();
            var res = controller.Post(browserId, boardCreate);
            res.AssertStatusCode();
            ScenarioContext.Current["createdboardid"] = res.EntityId();
        }
        
        [Then(@"a private BOARD named (.*) will be created")]
        public void ThenAPrivateBOARDNamedBoardWillBeCreated(string boardName)
        {
            ScenarioContext.Current.Pending();
        }
    }

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

    [DataContract]
    public class BoardCreateEditModel : ViewModelBase
    {
        public string Id { get; set; }

        [DisplayName("BoardName")]
        [RequiredWithMessage]
        public string BoardName { get; set; }

        [DisplayName("AllowOthersToPostFliers")]
        [DataMember]
        [RequiredWithMessage]
        public bool AllowOthersToPostFliers { get; set; }

        [DisplayName("RequireApprovalOfPostedFliers")]
        [DataMember]
        [RequiredWithMessage]
        public bool RequireApprovalOfPostedFliers { get; set; }

        [DisplayName("Location")]
        public LocationModel Location { get; set; }

    }
}
