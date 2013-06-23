using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Board;
using PostaFlya.Models.Flier;
using Website.Application.Content;
using Website.Common.Controller;
using Website.Common.Extension;
using Website.Common.Model.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class MyBoardFlierController : WebApiControllerBase
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly CommandBusInterface _commandBus;
        private readonly QueryChannelInterface _queryChannel;


        public MyBoardFlierController(GenericQueryServiceInterface queryService
                                      , BlobStorageInterface blobStorage
                                      , CommandBusInterface commandBus, QueryChannelInterface queryChannel)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
            _commandBus = commandBus;
            _queryChannel = queryChannel;
        }

        public List<BoardFlierModel> Get(string browserId, string boardId, BoardFlierStatus status)
        {
            var boardFlier = _queryService.FindAggregateEntities<BoardFlier>(boardId);
            return boardFlier
                .Where( bf=> status == BoardFlierStatus.UnKnown || bf.Status == status)
                .Select(bf => new BoardFlierModel()
                    {
                        BoardFlier = _queryChannel.ToViewModel<BulletinFlierSummaryModel>(_queryService.FindById<Flier>(bf.FlierId), null),
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