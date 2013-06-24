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
            return null;
        }

        public HttpResponseMessage Put(string browserId, EditBoardFlierModel boardEdit)
        {

            return null;
        }
    }
}