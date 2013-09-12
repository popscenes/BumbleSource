using System.Collections.Generic;
using System.Linq;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Indexes;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.DomainQuery.Board
{
    public class FindBoardByAdminEmailQueryHandler : QueryHandlerInterface<FindBoardsByAdminEmailQuery, List<Domain.Boards.Board>>
    {
        private readonly TableIndexServiceInterface _indexService;
        private readonly GenericQueryServiceInterface _queryService;

        public FindBoardByAdminEmailQueryHandler(TableIndexServiceInterface indexService, GenericQueryServiceInterface queryService)
        {
            _indexService = indexService;
            _queryService = queryService;
        }

        public List<Domain.Boards.Board> Query(FindBoardsByAdminEmailQuery argument)
        {

            var entries = _indexService.FindEntitiesByIndex<BoardInterface, StorageTableKey>(
                new BoardAdminEmailIndex(), argument.AdminEmail);
            return _queryService.FindByIds<Domain.Boards.Board>(entries.Select( _ => _.RowKey.ExtractEntityIdFromRowKey())).ToList();
        }
    }
}
