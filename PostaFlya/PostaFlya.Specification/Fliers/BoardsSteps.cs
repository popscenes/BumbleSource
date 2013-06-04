using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using PostaFlya.Controllers;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Venue;
using PostaFlya.Mocks.Domain.Data;
using PostaFlya.Models.Board;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using PostaFlya.Specification.Util;
using TechTalk.SpecFlow;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
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
                    TypeOfBoard = (BoardTypeEnum)Enum.Parse(typeof(BoardTypeEnum), table.Rows[0]["TypeOfBoard"])
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

        private void GivenThereIsAPublicBoardForBrowserNamed(string browserId, string targetBoard, bool requiresApproval = true)
        {
            var boardCreate = new BoardCreateEditModel()
            {
                BoardName = targetBoard,
                AllowOthersToPostFliers = true,
                RequireApprovalOfPostedFliers = requiresApproval,
            };

            WhenABrowserSubmitsTheFollowingDataForTheBOARD(boardCreate, browserId);
            ThenABOARDNamedBoardWillBeCreated(targetBoard);
            ThenTheBOARDWillAllowOthersToPost();
            ThenTheBOARDWillRequireApprovalForPostedFliers();
            ThenTheBOARDHasStatus(BoardStatus.Approved);
        }

        [Given(@"There is a public board named (.*) that doesn't require approval")]
        public void GivenThereIsAPublicBoardThatDoesntRequireApprovalNamed(string targetBoard)
        {
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            GivenThereIsAPublicBoardForBrowserNamed(browserId, targetBoard, false);
        }

        [Given(@"There is a venue board named (.*)")]
        public void GivenThereIsAVenueBoardNamed(string boardName)
        {
            var bus = SpecUtil.CurrIocKernel.Get<CommandBusInterface>();
            var createBoardCommand = new CreateBoardCommand()
            {

                BrowserId = Guid.Empty.ToString(),
                BoardName = boardName,
                AllowOthersToPostFliers = true,
                RequireApprovalOfPostedFliers = false,
                BoardTypeEnum = BoardTypeEnum.VenueBoard,
                SourceInformation = SpecUtil.CurrIocKernel.Get<VenueInformation>()
            };


            var ret = bus.Send(createBoardCommand) as MsgResponse;
            ScenarioContext.Current["createdboardid"] = ret.GetEntityId();
            ThenABOARDNamedBoardWillBeCreated(boardName);
            ThenTheBOARDWillAllowOthersToPost();
            ThenTheBOARDHasStatus(BoardStatus.Approved);
        }

        [Given(@"there is a public board named (.*) that requires approval")]
        public void GivenThereIsAPublicBoardThatRequiresApprovalNamed(string targetBoard)
        {   
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            GivenThereIsAPublicBoardForBrowserNamed(browserId, targetBoard);
        }

        [Given(@"there is an approved public board named (.*)")]
        public void GivenThereIsAnApprovedPublicBoardNamed(string targetBoard)
        {
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            GivenThereIsAPublicBoardForBrowserNamed(browserId, targetBoard);
            _common.GivenIHaveRole(Role.Admin.ToString());
            WhenIApproveTheBOARD();
            ThenTheBOARDHasStatus(BoardStatus.Approved);
        }

        [Given(@"I have created a public board named (.*) that requires approval")]
        public void GivenIHaveCreatedAPublicBoardThatRequiresApprovalNamed(string targetBoard)
        {
            var browserId = SpecUtil.GetCurrBrowser().Browser.Id;
            GivenThereIsAPublicBoardForBrowserNamed(browserId, targetBoard);
        }

        [Given(@"I have created an approved public board named (.*)")]
        public void GivenIHaveCreatedAnApprovedPublicBoardNamed(string targetBoard)
        {
            if (SpecUtil.GetCurrBrowser().Browser == null)
                ScenarioContext.Current["browserid"] =
                    SpecUtil.CurrIocKernel.Get<BrowserInterface>(ctx => ctx.Has("postadefaultbrowser"));

            var browserId = SpecUtil.GetCurrBrowser().Browser.Id;
            GivenThereIsAPublicBoardForBrowserNamed(browserId, targetBoard);
            _common.GivenIHaveRole(Role.Admin.ToString());
            WhenIApproveTheBOARD();
            ThenTheBOARDHasStatus(BoardStatus.Approved);
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
            flier = queryService.FindById<Flier>(flier.Id);
            ScenarioContext.Current["flier"] = flier;

            BoardInterface board = null;
            board = ScenarioContext.Current["board"] as BoardInterface;

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

            var ret = res.SingleOrDefault(bf => bf.BoardFlier.Id == flier.Id);
            Assert.IsNotNull(ret);

            var updateRes = controller.Put(browserId,
                           new EditBoardFlierModel()
                               {BoardId = board.Id, FlierId = ret.BoardFlier.Id, Status = BoardFlierStatus.Approved});

            updateRes.AssertStatusCode();
        }


        [Given(@"There is a FLIER that is Approved on a Board")]
        public void GivenThereIsAFLIERThatIsApprovedOnABoard()
        {
            GivenIHaveCreatedAPublicBoardThatRequiresApprovalNamed("testBoard");
            new FlierSteps().GivenABrowserHasCreatedAFlier();
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

        [Then(@"the BOARD will have the status (.*)")]
        public void ThenTheBOARDWillHaveTheStatusPendingApproval(BoardStatus status)
        {
            var board = ScenarioContext.Current["board"] as BoardInterface;

            Assert.That(board.Status, Is.EqualTo(status));
        }

        [Given(@"the BOARD has the status (.*)")]
        public void ThenTheBOARDHasStatus(BoardStatus status)
        {
            var board = ScenarioContext.Current["board"] as BoardInterface;
            var repo = SpecUtil.CurrIocKernel.Get<GenericRepositoryInterface>();
            repo.UpdateEntity<Board>(board.Id, boardUp => boardUp.Status = status);
            board.Status = status;

        }

        [When(@"I approve the BOARD")]
        public void WhenIApproveTheBOARD()
        {
            var board = ScenarioContext.Current["board"] as BoardInterface;
            var controller = SpecUtil.GetApiController<MyBoardsController>();
            var editModel = new BoardCreateEditModel()
                {
                    Id = board.Id,
                    Status = BoardStatus.Approved
                };         
            
            var browserId = SpecUtil.GetCurrBrowser().Browser.Id;

            var res = controller.Put(browserId, editModel);
            res.AssertStatusCode();

            var query = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            ScenarioContext.Current["board"] = query.FindById<Board>(board.Id);
        }


        [Given(@"There is no Board for a Venue")]
        public void GivenThereIsNoBoardForAVenue()
        {
           var venueInfo = SpecUtil.CurrIocKernel.Get<VenueInformation>(ib => ib.Get<bool>("default"));
           var qc = SpecUtil.CurrIocKernel.Get<QueryChannelInterface>();
           var ret = qc.Query(new FindBoardForVenueQuery() {VenueInformation = venueInfo}, (Board)null);
           Assert.That(ret, Is.Null);
        }

        [Then(@"a Venue BOARD will be created")]
        public void ThenAVenueBOARDWillBeCreated()
        {
            var venueInfo = SpecUtil.CurrIocKernel.Get<VenueInformation>(ib => ib.Get<bool>("default"));
            var qc = SpecUtil.CurrIocKernel.Get<QueryChannelInterface>();
            var ret = qc.Query(new FindBoardForVenueQuery() { VenueInformation = venueInfo }, (Board)null);
            Assert.That(ret, Is.Not.Null);
            Assert.That(ret.Name, Is.EqualTo(venueInfo.PlaceName));
            ScenarioContext.Current["board"] = ret;
        }

        [Given("There is a Board for a Venue with a Flier")]
        public void GivenThereIsABoardForAVenueWithAFlier()
        {
            GivenThereIsNoBoardForAVenue();
            _common.GivenIamAParticipantWithRole("Admin");
            new FlierSteps().WhenICreateAnAnonymousFlier();
            ThenAVenueBOARDWillBeCreated();
            ThenItWillBeAMemberOfTheBoardWithAStatusOf(BoardFlierStatus.Approved);
        }

        [Then(@"The Board will have (.*) Fliers")]
        public void ThenTheBoardWillHaveFliers(int numoffliers)
        {
            var queryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();

            var board = ScenarioContext.Current["board"] as BoardInterface;
            var boardFlier = queryService.FindAggregateEntities<BoardFlier>(board.Id);
            Assert.That(boardFlier.Count(), Is.EqualTo(numoffliers));
        }

        [When(@"I navigate to the public view page for that Board")]
        public void WhenINavigateToThePublicViewPageForThatBoard()
        {
            var board = ScenarioContext.Current["board"] as BoardInterface;
            var controller = SpecUtil.GetController<BoardController>();
            SpecUtil.ControllerResult = controller.Get(board.FriendlyId);
        }

        [Then(@"I will see the Information for that Board")]
        public void ThenIWillSeeTheInformationForThatBoard()
        {
            var board = ScenarioContext.Current["board"] as BoardInterface;
            var res = SpecUtil.ControllerResult as ViewResult;
            var bres = res.Model as BoardPageViewModel;
            Assert.That(bres, Is.Not.Null);
            Assert.That(bres.FriendlyId, Is.EqualTo(board.FriendlyId));
            Assert.That(bres.Description, Is.EqualTo(board.Description));
            Assert.That(bres.BoardTypeEnum, Is.EqualTo(board.BoardTypeEnum));
        }

        [Then(@"I will see the Fliers on that Board")]
        public void ThenIWillSeeTheFliersOnThatBoard()
        {
            var bulletinController = SpecUtil.GetApiController<BulletinApiController>();
            var board = ScenarioContext.Current["board"] as BoardInterface;
            var dateFilter = ScenarioContext.Current.ContainsKey("eventfilterdate") ? ScenarioContext.Current["eventfilterdate"] as DateTime? : null;


            SpecUtil.ControllerResult = bulletinController
                .Get(new BulletinGetRequestModel()
                    {
                        Loc = new LocationModel(),
                        Count = 30,
                        Board = board.Id,
                        Date = dateFilter,
                        Tags = ""
                    });
               

            var fliers = SpecUtil.ControllerResult as IList<BulletinFlierModel>;
            var flier = fliers.FirstOrDefault();
            Assert.IsNotNull(flier, "no fliers in context");
        }


    }
}
