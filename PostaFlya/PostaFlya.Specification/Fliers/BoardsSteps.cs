using System;
using System.Linq;
using NUnit.Framework;
using Ninject;
using PostaFlya.Controllers;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using PostaFlya.Mocks.Domain.Data;
using PostaFlya.Models.Board;
using PostaFlya.Models.Flier;
using PostaFlya.Specification.Util;
using TechTalk.SpecFlow;
using Website.Infrastructure.Query;

namespace PostaFlya.Specification.Fliers
{
    [Binding]
    public class BoardsSteps
    {
        private readonly CommonSteps _common = new CommonSteps();

        [When(@"I submit the following data for the BOARD:")]
        public void WhenISubmitsTheFollowingDataForTheBOARD(Table table)
        {
            var boardCreate = new BoardCreateEditModel()
                {
                    BoardName = table.Rows[0]["BoardName"],
                    AllowOthersToPostFliers = Boolean.Parse(table.Rows[0]["AcceptOthersFliers"]),
                    RequireApprovalOfPostedFliers = Boolean.Parse(table.Rows[0]["RequireApprovalForFliers"]),
                };

            WhenABrowserSubmitsTheFollowingDataForTheBOARD(boardCreate, SpecUtil.GetCurrBrowser().Browser.Id);
        }

        public void WhenABrowserSubmitsTheFollowingDataForTheBOARD(BoardCreateEditModel boardCreate, string browserId)
        {
            var controller = SpecUtil.GetApiController<MyBoardsController>();
            var res = controller.Post(browserId, boardCreate);
            res.AssertStatusCode();
            ScenarioContext.Current["createdboardid"] = res.EntityId();
        }
        
        [Then(@"a BOARD named (.*) will be created")]
        public void ThenABOARDNamedBoardWillBeCreated(string boardName)
        {
            var queryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var board = queryService.FindById<Board>(ScenarioContext.Current["createdboardid"] as string);
            Assert.IsNotNull(board);

            var board2 = queryService.FindByFriendlyId<Board>(boardName.ToLower());
            Assert.IsNotNull(board2);

            BoardTestData.AssertStoreRetrieve(board, board2);

            ScenarioContext.Current["board"] = board;
        }

        [Then(@"the BOARD will allow Others to post FLIERS")]
        public void ThenTheBOARDWillAllowOthersToPost()
        {
            var board = ScenarioContext.Current["board"] as BoardInterface;
            Assert.IsNotNull(board);
            Assert.That(board.AllowOthersToPostFliers, Is.True);
        }


        [Then(@"the BOARD will require approval for posted FLIERS")]
        public void ThenTheBOARDWillRequireApprovalForPostedFliers()
        {
            var board = ScenarioContext.Current["board"] as BoardInterface;
            Assert.IsNotNull(board);
            Assert.That(board.AllowOthersToPostFliers, Is.True);
        }

        private void GivenThereIsAPublicBoardForBrowserThatRequiresApprovalNamed(string browserId, string targetBoard)
        {
            var boardCreate = new BoardCreateEditModel()
            {
                BoardName = targetBoard,
                AllowOthersToPostFliers = true,
                RequireApprovalOfPostedFliers = true,
            };

            WhenABrowserSubmitsTheFollowingDataForTheBOARD(boardCreate, browserId);
            ThenABOARDNamedBoardWillBeCreated(targetBoard);
            ThenTheBOARDWillAllowOthersToPost();
            ThenTheBOARDWillRequireApprovalForPostedFliers();
        }

        [Given(@"there is a public board named (.*) that requires approval")]
        public void GivenThereIsAPublicBoardThatRequiresApprovalNamed(string targetBoard)
        {   
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            GivenThereIsAPublicBoardForBrowserThatRequiresApprovalNamed(browserId, targetBoard);
        }

        [Given(@"I have created a public board named (.*) that requires approval")]
        public void GivenIHaveCreatedAPublicBoardThatRequiresApprovalNamed(string targetBoard)
        {
            var browserId = SpecUtil.GetCurrBrowser().Browser.Id;
            GivenThereIsAPublicBoardForBrowserThatRequiresApprovalNamed(browserId, targetBoard);
        }

        [When(@"I add the FLIER to the board")]
        public void WhenIAddTheFLIERToTheBoard()
        {
            var browserId = SpecUtil.GetCurrBrowser().Browser.Id;
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var board = ScenarioContext.Current["board"] as BoardInterface;

            var controller = SpecUtil.GetApiController<BoardFlierController>();

            var addFlierModel = new AddBoardFlierModel()
            {
                BoardId = board.Id,
                FlierId = flier.Id
            };
            var res = controller.Post(browserId, addFlierModel);
            res.AssertStatusCode();
        }

        [When(@"A BROWSER adds the FLIER to the board")]
        public void WhenABrowserAddsTheFLIERToTheBoard()
        {
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var board = ScenarioContext.Current["board"] as BoardInterface;

            var controller = SpecUtil.GetApiController<MyFliersController>();

            var addFlierModel = flier.ToCreateModel();
            addFlierModel.BoardList.Add(board.Id);
            var res = controller.Put(browserId, addFlierModel);
            res.AssertStatusCode();
        }

        [Given(@"A BROWSER has added the FLIER to the board")]
        public void GivenABrowserAddsTheFLIERToTheBoard()
        {
            WhenABrowserAddsTheFLIERToTheBoard();
            ThenItWillBeAMemberOfTheBoardWithAStatusOf(BoardFlierStatus.PendingApproval);
        }

        [Then(@"The FLIER will be a member of the board with a status of (.*)")]
        public void ThenItWillBeAMemberOfTheBoardWithAStatusOf(BoardFlierStatus status)
        {
            var queryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var board = ScenarioContext.Current["board"] as BoardInterface;

            var boardFlier = queryService.FindAggregateEntities<BoardFlier>(board.Id);
            Assert.That(boardFlier.Count(), Is.GreaterThan(0));
            Assert.That(boardFlier.All(bf => bf != null));
            var ret = boardFlier.SingleOrDefault(bf => bf.FlierId == flier.Id);
            Assert.IsNotNull(ret);
            Assert.That(ret.Status, Is.EqualTo(status));
            ScenarioContext.Current["flier"] = queryService.FindById<Flier>(flier.Id);
        }

        [When(@"I approve the FLIER")]
        public void WhenIApproveTheFLIER()
        {
            var board = ScenarioContext.Current["board"] as BoardInterface;
            var flier = ScenarioContext.Current["flier"] as FlierInterface;

            var controller = SpecUtil.GetApiController<MyBoardFlierController>();
            var browserId = SpecUtil.GetCurrBrowser().Browser.Id;
            var res = controller.Get(browserId, board.Id, BoardFlierStatus.PendingApproval);

            var ret = res.SingleOrDefault(bf => bf.Flier.Id == flier.Id);
            Assert.IsNotNull(ret);

            var updateRes = controller.Put(browserId,
                           new EditBoardFlierModel()
                               {BoardId = board.Id, FlierId = ret.Flier.Id, Status = BoardFlierStatus.Approved});

            updateRes.AssertStatusCode();
        }


        [Given(@"There is a FLIER that is Approved on a Board")]
        public void GivenThereIsAFLIERThatIsApprovedOnABoard()
        {
            GivenIHaveCreatedAPublicBoardThatRequiresApprovalNamed("testBoard");
            new FlierSteps().GivenABrowserHasCreatedAFlierofBehaviour();
            GivenABrowserAddsTheFLIERToTheBoard();
            WhenIApproveTheFLIER();
            ThenItWillBeAMemberOfTheBoardWithAStatusOf(BoardFlierStatus.Approved);
        }

        [When(@"A BROWSER modifies the FLIER on a Board")]
        public void WhenABROWSERModifiesTheFLIEROnABoard()
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var controller = SpecUtil.GetApiController<MyFliersController>();

            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            var edit = flier.ToCreateModel();
            var updateRes = controller.Put(browserId, edit);
            updateRes.AssertStatusCode();

        }


    }
}
