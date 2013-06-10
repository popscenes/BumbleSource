using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Behaviour.Query;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Board;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using Website.Application.Content;
using Website.Common.Controller;
using Website.Common.Extension;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class MyBoardFlierController : WebApiControllerBase
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourQueryServiceInterface _behaviourQueryService;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;
        private readonly CommandBusInterface _commandBus;


        public MyBoardFlierController(GenericQueryServiceInterface queryService
                                      , BlobStorageInterface blobStorage
                                      , FlierBehaviourQueryServiceInterface behaviourQueryService
                                      , FlierBehaviourViewModelFactoryInterface viewModelFactory, CommandBusInterface commandBus)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
            _behaviourQueryService = behaviourQueryService;
            _viewModelFactory = viewModelFactory;
            _commandBus = commandBus;
        }

        public List<BoardFlierModel> Get(string browserId, string boardId, BoardFlierStatus status)
        {
            var boardFlier = _queryService.FindAggregateEntities<BoardFlier>(boardId);
            return boardFlier
                .Where( bf=> status == BoardFlierStatus.UnKnown || bf.Status == status)
                .Select(bf => new BoardFlierModel()
                    {
                        BoardFlier = _viewModelFactory.GetBulletinViewModel(_queryService.FindById<Flier>(bf.FlierId), false)
                                  .GetImageUrl(_blobStorage),
                        BoardId = bf.AggregateId,
                        Status = bf.Status
                    }).ToList();
        }

        public HttpResponseMessage Put(string browserId, EditBoardFlierModel boardEdit)
        {

            var editBoardFlierCommand = new EditBoardFlierCommand()
                {
                    BrowserId = browserId,
                    FlierId = boardEdit.FlierId,
                    BoardId = boardEdit.BoardId,
                    Status = boardEdit.Status               
                };

            var res = _commandBus.Send(editBoardFlierCommand);
            return this.GetResponseForRes(res);
        }
    }
}